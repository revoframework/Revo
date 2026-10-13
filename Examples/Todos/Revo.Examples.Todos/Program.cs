using Microsoft.EntityFrameworkCore;
using Revo.AspNetCore.Configuration;
using Revo.Core.Configuration;
using Revo.EFCore.Configuration;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.Examples.Todos;
using Revo.Infrastructure.DataAccess.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddOpenApi();

builder.Services.AddRevo(configurator =>
    configurator
        .ConfigureDefaults()
        .ConfigureCore(core => core.UseAllAppDomainAssembliesForTypeDiscovery())
        .UseAspNetCore()
        .UseEFCoreDataAccess(
            ConfigureDatabase,
            advancedAction: config =>
            {
                config
                    .AddConvention<BaseTypeAttributeConvention>(-200)
                    .AddConvention<IdColumnsPrefixedWithTableNameConvention>(-110)
                    .AddConvention<PrefixConvention>(-9)
                    .AddConvention<SnakeCaseTableNamesConvention>(1)
                    .AddConvention<SnakeCaseColumnNamesConvention>(1)
                    .AddConvention<LowerCaseConvention>(2);
            })
        .UseEFCoreInfrastructure()
    .ConfigureServices(services =>
    {
        services.AddSingleton(new ResourceDatabaseMigrationDiscoveryAssembly(
            typeof(Program).Assembly, "Sql"));
    }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseStatusCodePages();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapRazorPages();

app.Run();

// SQLite by default (nothing to install), PostgreSQL with "DatabaseProvider": "PostgreSQL"
// (connection strings "ConnectionStrings:Sqlite" and "ConnectionStrings:PostgreSQL")
void ConfigureDatabase(DbContextOptionsBuilder contextBuilder)
{
    var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";

    if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        contextBuilder
            .UseSqlite(builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=todos.db")
            .AddInterceptors(new SqliteConcurrencyInterceptor());
    }
    else if (string.Equals(provider, "PostgreSQL", StringComparison.OrdinalIgnoreCase))
    {
        contextBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
    }
    else
    {
        throw new InvalidOperationException(
            $"Unsupported DatabaseProvider '{provider}', use Sqlite or PostgreSQL");
    }
}

// for WebApplicationFactory in the tests
public partial class Program;
