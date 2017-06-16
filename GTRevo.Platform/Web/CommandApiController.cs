using System.Web.Http;
using GTRevo.Commands;
using Ninject;

namespace GTRevo.Platform.Web
{
    public abstract class CommandApiController : ApiController
    {
        [Inject]
        public ICommandBus CommandBus { get; set; }
    }
}
