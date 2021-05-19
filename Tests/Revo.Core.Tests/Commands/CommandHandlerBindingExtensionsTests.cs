using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Ninject;
using Ninject.Planning.Bindings.Resolvers;
using Revo.Core.Commands;
using Revo.Core.Core;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class CommandHandlerBindingExtensionsTests
    {
        [Fact]
        public void BindCommandHandler_GenericHandler()
        {
            var kernel = new StandardKernel();
            kernel.Components.Add<IBindingResolver, ContravariantBindingResolver>();

            //var chIntf = typeof(ICommandHandler<>).MakeGenericType(typeof(TestGenericCommand<>));
            //kernel.Bind(chIntf).To(typeof(TestGenericCommandHandler<>)).InTransientScope();

            kernel.BindCommandHandler<TestGenericCommandHandler<string>>();

            var commandHandler = kernel.Get<ICommandHandler<TestGenericCommand<string>>>();
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