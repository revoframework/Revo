using FluentAssertions;
using Revo.Core.Commands;
using Revo.Core.Transactions;
using NSubstitute;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class CommandContextTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            var command = Substitute.For<ICommandBase>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var sut = new CommandContext(command, unitOfWork);

            sut.CurrentCommand.Should().Be(command);
            sut.UnitOfWork.Should().Be(unitOfWork);
        }
    }
}
