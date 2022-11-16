using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Revo.Examples.BlazorWasmTodos;

var config = new NLog.Config.LoggingConfiguration();
var consoleTarget = new NLog.Targets.ConsoleTarget("console");
config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget);
NLog.LogManager.Configuration = config;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);
await startup.Configure();

await builder.Build().RunAsync();
