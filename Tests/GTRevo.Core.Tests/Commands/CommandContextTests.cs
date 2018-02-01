using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GTRevo.Core.Commands;
using GTRevo.Core.Transactions;
using NSubstitute;
using Xunit;

namespace GTRevo.Core.Tests.Commands
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
