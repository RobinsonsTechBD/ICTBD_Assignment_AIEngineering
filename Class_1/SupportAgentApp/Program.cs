var builder = WebApplication.CreateBuilder(args);

// Add standard MVC services
builder.Services.AddControllersWithViews();

// Register a clean, standard HTTP client to communicate with Ollama
builder.Services.AddHttpClient("OllamaClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434");
    client.Timeout = TimeSpan.FromMinutes(5); // Gives your RAM plenty of time to warm up
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.ChatCompletion;
//using OllamaSharp;

//#pragma warning disable SKEXP0070

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// 1. Create a bulletproof HttpClient with a 5-minute timeout
//var customHttpClient = new HttpClient
//{
//    BaseAddress = new Uri("http://localhost:11434"),
//    Timeout = TimeSpan.FromMinutes(5)
//};

//// 2. Create the Ollama client manually
//var ollamaApiClient = new OllamaApiClient(customHttpClient, "llama3.1:latest");

//// 3. Register the Chat Service directly using the non-deprecated extension pattern
//builder.Services.AddKeyedSingleton<IChatCompletionService>("OllamaChat", ollamaApiClient.AsChatCompletionService());

//builder.Services.AddTransient<Kernel>(sp =>
//{
//    var kernelBuilder = Kernel.CreateBuilder();

//    // Add your tracking code tool registration here
//    kernelBuilder.Plugins.AddFromType<SupportAgentApp.Plugins.OrderPlugin>();

//    return kernelBuilder.Build();
//});

//var app = builder.Build();

//app.UseStaticFiles();
//app.UseRouting();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();