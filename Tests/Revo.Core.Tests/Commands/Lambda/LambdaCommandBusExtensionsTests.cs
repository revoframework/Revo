using System;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Commands.Lambda;
using Xunit;

namespace Revo.Core.Tests.Commands.Lambda
{
    public class LambdaCommandBusExtensionsTests
    {
        private ICommandBus commandBus;

        public LambdaCommandBusExtensionsTests()
        {
            commandBus = Substitute.For<ICommandBus>();
        }

        [Fact]
        public async Task SendCommand()
        {
            Func<object, Task> lambda = async param1 => {};
            await commandBus.SendLambdaCommandAsync(lambda);
            commandBus.Received(1).SendAsync(Arg.Is<LambdaCommand>(x => x.Delegate == lambda));
        }
        
        [Fact]
        public async Task SendResultCommand()
        {
            Func<object, Task<int>> lambda = async param1 => 1;
            await commandBus.SendLambdaCommandAsync(lambda);
            commandBus.Received(1).SendAsync(Arg.Is<LambdaResultCommand>(x => x.Delegate == lambda));
        }
    }
}