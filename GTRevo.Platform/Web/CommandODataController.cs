using System.Web.OData;
using System.Web.Http.Description;
using GTRevo.Core.Commands;
using Ninject;

namespace GTRevo.Platform.Web
{
    [ApiExplorerSettings(IgnoreApi = false)]
    public abstract class CommandODataController : ODataController
    {
        [Inject]
        public ICommandBus CommandBus { get; set; }
    }
}
