using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Revo.Core.Logging;

public static class GenericLoggerFactory
{
    /// <summary>
    /// Creates ILogger<T> loggers (non-generic ILoggerFactory.CreateLogger returns just ILogger).
    /// </summary>
    public static ILogger CreateGenericLogger(this ILoggerFactory loggerFactory, Type type)
    {
        var loggerConstructorInfo = typeof(Logger<>).MakeGenericType(type)
            .GetConstructor(BindingFlags.Public | BindingFlags.Instance, new[] { typeof(ILoggerFactory) });
        return (ILogger)loggerConstructorInfo.Invoke(new[] { loggerFactory });
    }
}