using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Commands;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class CommandHandlerBindingExtensionsTests
    {
        [Fact]
        public void AddCommandHandler_GenericHandler()
        {
            var services = new ServiceCollection();
            services.AddCommandHandler<TestGenericCommandHandler<string>>();

            using var provider = services.BuildServiceProvider();
            var commandHandler = provider.GetService<ICommandHandler<TestGenericCommand<string>>>();
            commandHandler.Should().NotBeNull();
        }

        public class TestGenericCommand<T> : ICommand
        {
        }

        public class TestGenericCommandHandler<T> : ICommandHandler<TestGenericCommand<T>>
        {
            public TestGenericCommandHandler()
            {
            }

            public virtual Task HandleAsync(TestGenericCommand<T> command, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}