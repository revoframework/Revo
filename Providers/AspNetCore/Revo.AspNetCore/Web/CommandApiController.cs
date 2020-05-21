using Microsoft.AspNetCore.Mvc;
using Ninject;
using Revo.Core.Commands;

namespace Revo.AspNetCore.Web
{
    public abstract class CommandApiController : ControllerBase
    {
        [Inject]
        public ICommandBus CommandBus { get; set; }

        [Inject]
        public ICommandGateway CommandGateway { get; set; }
    }
}
