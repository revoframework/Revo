using FluentAssertions;
using Microsoft.Extensions.Logging;
using Revo.Core.Logging;
using Xunit;

namespace Revo.Core.Tests.Logging;

public class GenericLoggerFactoryTests
{
    [Fact]
    public void ReturnsGenericLogger()
    {
        // generate test
        var loggerFactory = LoggerFactory.Create(builder => {});
        var result = loggerFactory.CreateGenericLogger(typeof(GenericLoggerFactoryTests));
        result.Should().BeAssignableTo<ILogger<GenericLoggerFactoryTests>>();
    }
}