using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Services;

public static class MultiRegistrationServiceCollectionExtensions
{
    public static void AddSingleton<TInterface1, TInterface2, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2
        where TInterface1 : class
        where TInterface2 : class
    {
        services.AddSingleton<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddSingleton<TInterface1, TInterface2, TInterface3, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2, TInterface3
        where TInterface1 : class
        where TInterface2 : class
        where TInterface3 : class
    {
        services.AddSingleton<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface3>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddSingleton<TInterface1, TInterface2, TInterface3, TInterface4, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2, TInterface3, TInterface4
        where TInterface1 : class
        where TInterface2 : class
        where TInterface3 : class
        where TInterface4 : class
    {
        services.AddSingleton<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface3>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface4>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddSingleton<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5
        where TInterface1 : class
        where TInterface2 : class
        where TInterface3 : class
        where TInterface4 : class
        where TInterface5 : class
    {
        services.AddSingleton<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface3>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface4>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface5>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddTransient<TInterface1, TInterface2, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2
        where TInterface1 : class
        where TInterface2 : class
    {
        services.AddTransient<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddTransient<TInterface1, TInterface2, TInterface3, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2, TInterface3
        where TInterface1 : class
        where TInterface2 : class
        where TInterface3 : class
    {
        services.AddTransient<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface3>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddTransient<TInterface1, TInterface2, TInterface3, TInterface4, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2, TInterface3, TInterface4
        where TInterface1 : class
        where TInterface2 : class
        where TInterface3 : class
        where TInterface4 : class
    {
        services.AddTransient<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface3>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface4>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddTransient<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5
        where TInterface1 : class
        where TInterface2 : class
        where TInterface3 : class
        where TInterface4 : class
        where TInterface5 : class
    {
        services.AddTransient<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface3>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface4>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface5>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddScoped<TInterface1, TInterface2, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2
        where TInterface1 : class
        where TInterface2 : class
    {
        services.AddScoped<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddScoped<TInterface1, TInterface2, TInterface3, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2, TInterface3
        where TInterface1 : class
        where TInterface2 : class
        where TInterface3 : class
    {
        services.AddScoped<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface3>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddScoped<TInterface1, TInterface2, TInterface3, TInterface4, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2, TInterface3, TInterface4
        where TInterface1 : class
        where TInterface2 : class
        where TInterface3 : class
        where TInterface4 : class
    {
        services.AddScoped<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface3>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface4>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }

    public static void AddScoped<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TService>(this IServiceCollection services)
        where TService : class, TInterface1, TInterface2, TInterface3, TInterface4, TInterface5
        where TInterface1 : class
        where TInterface2 : class
        where TInterface3 : class
        where TInterface4 : class
        where TInterface5 : class
    {
        services.AddScoped<TService>();
        services.AddTransient<TInterface1>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface2>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface3>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface4>(serviceProvider => serviceProvider.GetRequiredService<TService>());
        services.AddTransient<TInterface5>(serviceProvider => serviceProvider.GetRequiredService<TService>());
    }
}