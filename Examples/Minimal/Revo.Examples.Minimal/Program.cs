using Revo.AspNetCore.Configuration;
using Revo.Core.Commands;
using Revo.Core.Configuration;
using Revo.Examples.Minimal.Api;
using Revo.Examples.Minimal.Messages.Commands;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddRevo(configurator =>
    configurator
        .ConfigureDefaults()
        .ConfigureCore(core => core.UseAllAppDomainAssembliesForTypeDiscovery())
        .UseAspNetCore());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Text("Hello Revo!", "text/plain"));

app.MapPost("/todo-lists", async (CreateTodoListRequest dto, ICommandBus commandBus) =>
{
    await commandBus.SendAsync(new CreateTodoListCommand
    {
        Id = dto.Id,
        Name = dto.Name
    });
});

app.Run();
