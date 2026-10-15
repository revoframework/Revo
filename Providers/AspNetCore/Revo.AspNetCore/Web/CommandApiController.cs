using Microsoft.AspNetCore.Mvc;
using Revo.Core.Commands;

namespace Revo.AspNetCore.Web;

public abstract class CommandApiController(ICommandBus commandBus, ICommandGateway commandGateway) : ControllerBase
{
    public ICommandBus CommandBus { get; private set; } = commandBus;
    public ICommandGateway CommandGateway { get; private set; } = commandGateway;
}