using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using SupportAgentApp.Plugins;

namespace SupportAgentApp.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OrderPlugin _orderPlugin;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _orderPlugin = new OrderPlugin(); // Instantiate your project tool directly
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return Json(new { response = "Please enter a valid request." });
        }

        // 1. Direct Rule-Based Intercept Routing
        string upperMessage = userMessage.ToUpper();
        if (upperMessage.Contains("ORD123"))
        {
            var result = _orderPlugin.GetOrderStatus("ORD123");
            return Json(new { response = $"[System Action] {result}" });
        }
        if (upperMessage.Contains("ORD456"))
        {
            var result = _orderPlugin.GetOrderStatus("ORD456");
            return Json(new { response = $"[System Action] {result}" });
        }

        // 2. Fallback Direct JSON API Call to Ollama
        try
        {
            var httpClient = _httpClientFactory.CreateClient("OllamaClient");

            // Construct the exact payload expected by Ollama's native engine
            var requestBody = new
            {
                model = "llama3.1:latest", // Change to "phi3:latest" if using phi
                prompt = $"You are a helpful customer support agent for Robinsons TechBD. User asks: {userMessage}",
                stream = false // Set stream to false to get a clean single response back
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Post straight to the fundamental generation route
            var response = await httpClient.PostAsync("/api/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                return Json(new { response = $"System: Local AI returned error code {(int)response.StatusCode}." });
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            string aiText = doc.RootElement.GetProperty("response").GetString() ?? "No response received.";

            return Json(new { response = aiText });
        }
        catch (TaskCanceledException)
        {
            return Json(new { response = "System: Connection timeout. The local model is taking too long to load into RAM." });
        }
        catch (Exception ex)
        {
            return Json(new { response = $"System Error: Unable to connect to local AI server. {ex.Message}" });
        }
    }
}