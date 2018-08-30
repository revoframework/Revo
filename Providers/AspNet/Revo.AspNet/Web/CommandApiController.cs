using System.Web.Http;
using Ninject;
using Revo.Core.Commands;

namespace Revo.AspNet.Web
{
    public abstract class CommandApiController : ApiController
    {
        [Inject]
        public ICommandBus CommandBus { get; set; }
    }
}
