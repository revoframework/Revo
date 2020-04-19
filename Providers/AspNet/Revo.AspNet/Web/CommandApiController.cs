using System.Web.Http;
using Ninject;
using Revo.Core.Commands;

namespace Revo.AspNet.Web
{
    public abstract class CommandApiController : ApiController
    {
        [Inject]
        public ILocalCommandBus CommandBus { get; set; }

        [Inject]
        public ICommandGateway CommandGateway { get; set; }
    }
}
