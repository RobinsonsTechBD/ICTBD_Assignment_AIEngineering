using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using AISmartHub.Api.Data;
using AISmartHub.Api.Models;

namespace AISmartHub.Api.Controllers;

// 1. Create a clean DTO structure to handle incoming requests without binding crashes
public class TextRequestDto
{
    public string Prompt { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class AiHubController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public AiHubController(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    // 2. TEXT-TO-TEXT (Using our custom timeout client configuration cleanly)
    [HttpPost("text-to-text")]
    public async Task<IActionResult> TextToText([FromBody] TextRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest("Prompt cannot be empty.");
        }

        try
        {
            // Call the client we configured with the 5-minute timeout in Program.cs
            var httpClient = _httpClientFactory.CreateClient("OllamaClient");

            var requestBody = new { model = "llama3.1:latest", prompt = request.Prompt, stream = false };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Post straight to the API route endpoint relative to the BaseAddress
            var response = await httpClient.PostAsync("/api/generate", content);

            if (!response.IsSuccessStatusCode) return StatusCode(500, "Ollama connection failed.");

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            string aiResponse = doc.RootElement.GetProperty("response").GetString() ?? "";

            // Save transaction history records into MS SQL
            var log = new AIInteraction { InteractionType = "TextToText", InputData = request.Prompt, OutputData = aiResponse };
            _context.AIInteractions.Add(log);
            await _context.SaveChangesAsync();

            return Ok(log);
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, "Gateway Timeout: Ollama took longer than 5 minutes to respond.");
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    // 3. LOG FRONTEND SPEECH TRANSACTIONS
    [HttpPost("log-interaction")]
    public async Task<IActionResult> LogInteraction([FromBody] AIInteraction model)
    {
        try
        {
            model.CreatedAt = DateTime.UtcNow;
            _context.AIInteractions.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    // 4. GET HISTORY LOGS
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var logs = await _context.AIInteractions.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return Ok(logs);
    }
}


//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Text;
//using System.Text.Json;
//using AISmartHub.Api.Data;
//using AISmartHub.Api.Models;

//namespace AISmartHub.Api.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class AiHubController : ControllerBase
//{
//    private readonly AppDbContext _context;
//    private readonly IHttpClientFactory _httpClientFactory;

//    public AiHubController(AppDbContext context, IHttpClientFactory httpClientFactory)
//    {
//        _context = context;
//        _httpClientFactory = httpClientFactory;
//    }

//    // 1. TEXT-TO-TEXT (Talks directly to local Ollama instance)
//    [HttpPost("text-to-text")]
//    public async Task<IActionResult> TextToText([FromBody] string prompt)
//    {
//        try
//        {
//            var httpClient = _httpClientFactory.CreateClient();
//            var requestBody = new { model = "llama3.1:latest", prompt = prompt, stream = false };

//            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
//            var response = await httpClient.PostAsync("http://localhost:11434/api/generate", content);

//            if (!response.IsSuccessStatusCode) return StatusCode(500, "Ollama connection failed.");

//            var responseJson = await response.Content.ReadAsStringAsync();
//            using var doc = JsonDocument.Parse(responseJson);
//            string aiResponse = doc.RootElement.GetProperty("response").GetString() ?? "";

//            // Save to MS SQL Database Log
//            var log = new AIInteraction { InteractionType = "TextToText", InputData = prompt, OutputData = aiResponse };
//            _context.AIInteractions.Add(log);
//            await _context.SaveChangesAsync();

//            return Ok(log);
//        }
//        catch (Exception ex) { return StatusCode(500, ex.Message); }
//    }

//    // 2. LOG FRONTEND SPEECH TRANSACTIONS (Saves STT & TTS history into MS SQL)
//    [HttpPost("log-interaction")]
//    public async Task<IActionResult> LogInteraction([FromBody] AIInteraction model)
//    {
//        try
//        {
//            model.CreatedAt = DateTime.UtcNow;
//            _context.AIInteractions.Add(model);
//            await _context.SaveChangesAsync();
//            return Ok(model);
//        }
//        catch (Exception ex) { return StatusCode(500, ex.Message); }
//    }

//    // 3. GET HISTORY
//    [HttpGet("history")]
//    public async Task<IActionResult> GetHistory()
//    {
//        var logs = await _context.AIInteractions.OrderByDescending(x => x.CreatedAt).ToListAsync();
//        return Ok(logs);
//    }
//}






//using AISmartHub.Api.Data;
//using AISmartHub.Api.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Text.Json;
//using System.Text;
//using Microsoft.EntityFrameworkCore;
//using System.Speech.Synthesis;
//using System.Speech.Recognition;


//namespace AISmartHub.Api.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AiHubController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly IWebHostEnvironment _env;

//        public AiHubController(AppDbContext context, IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
//        {
//            _context = context;
//            _httpClientFactory = httpClientFactory;
//            _env = env;
//        }

//        [HttpPost("text-to-text")]
//        public async Task<IActionResult> TextToText([FromBody] string prompt)
//        {
//            try
//            {
//                var httpClient = _httpClientFactory.CreateClient();
//                var requestBody = new { 
//                    model = "llama3.1:latest", 
//                    prompt = prompt, 
//                    stream = false };

//                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
//                var response = await httpClient.PostAsync("http://localhost:11434/api/generate", content);

//                if (!response.IsSuccessStatusCode) return StatusCode(500, "Ollama dropped the connection.");

//                var responseJson = await response.Content.ReadAsStringAsync();
//                using var doc = JsonDocument.Parse(responseJson);
//                string aiResponse = doc.RootElement.GetProperty("response").GetString() ?? "";

//                // Save to MS SQL Database Log
//                var log = new AIInteraction { InteractionType = "TextToText", InputData = prompt, OutputData = aiResponse };
//                _context.AIInteractions.Add(log);
//                await _context.SaveChangesAsync();

//                return Ok(log);
//            }
//            catch (Exception ex) { return StatusCode(500, ex.Message); }
//        }

//        // 2. TEXT TO SPEECH (Generates a playable audio WAV file locally)
//        [HttpPost("text-to-speech")]
//        public async Task<IActionResult> TextToSpeech([FromBody] string text)
//        {
//            try
//            {
//                string fileName = $"tts_{Guid.NewGuid()}.wav";
//                string wwwrootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

//                if (!Directory.Exists(wwwrootPath)) Directory.CreateDirectory(wwwrootPath);
//                string filePath = Path.Combine(wwwrootPath, fileName);

//                // Using local system architecture thread wrapper to avoid UI blockages
//                using (var synthesizer = new SpeechSynthesizer())
//                {
//                    synthesizer.SetOutputToWaveFile(filePath);
//                    synthesizer.Speak(text);
//                }

//                string relativeAudioUrl = $"/{fileName}";

//                // Save to SQL Database Log
//                var log = new AIInteraction { InteractionType = "TextToSpeech", InputData = text, OutputData = relativeAudioUrl };
//                _context.AIInteractions.Add(log);
//                await _context.SaveChangesAsync();

//                return Ok(log);
//            }
//            catch (Exception ex) { return StatusCode(500, ex.Message); }
//        }

//        // 3. SPEECH TO TEXT (Accepts recorded user audio files from Angular)
//        [HttpPost("speech-to-text")]
//        public async Task<IActionResult> SpeechToText(IFormFile audioFile)
//        {
//            if (audioFile == null || audioFile.Length == 0)
//            {
//                return BadRequest("No audio file provided.");
//            }

//            try
//            {
//                string fileName = $"stt_{Guid.NewGuid()}.wav";
//                string wwwrootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

//                if (!Directory.Exists(wwwrootPath)) Directory.CreateDirectory(wwwrootPath);
//                string filePath = Path.Combine(wwwrootPath, fileName);

//                // Save the incoming frontend recording onto our hosting server storage layer
//                using (var stream = new FileStream(filePath, FileMode.Create))
//                {
//                    await audioFile.CopyToAsync(stream);
//                }

//                string transcribedText = "";

//                // Use lightweight internal engine wrapper thread to extract textual contents
//                using (var recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US")))
//                {
//                    recognizer.LoadGrammar(new DictationGrammar());
//                    recognizer.SetInputToWaveFile(filePath);

//                    RecognitionResult result = recognizer.Recognize();
//                    transcribedText = result != null ? result.Text : "[Unrecognized Audio Speech Content]";
//                }

//                // Save transaction parameters right down into MS SQL Server records
//                var log = new AIInteraction
//                {
//                    InteractionType = "SpeechToText",
//                    InputData = $"Audio Log Saved: /{fileName}",
//                    OutputData = transcribedText
//                };
//                _context.AIInteractions.Add(log);
//                await _context.SaveChangesAsync();

//                return Ok(log);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Audio engine parsing exception occurred: {ex.Message}");
//            }
//        }

//        // 3. GET HISTORY (Fetches tracking records)
//        [HttpGet("history")]
//        public async Task<IActionResult> GetHistory()
//        {
//            var logs = await _context.AIInteractions.OrderByDescending(x => x.CreatedAt).ToListAsync();
//            return Ok(logs);
//        }
//    }
//}
