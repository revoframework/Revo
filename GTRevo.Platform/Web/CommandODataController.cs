using System.Web.Http;
using System.Web.OData;
using System.Web.Http.Description;
using System.Web.OData.Query;
using GTRevo.Core.Commands;
using GTRevo.Platform.Web;
using Ninject;

namespace GTRevo.Platform.Web
{
    public abstract class CommandODataController : CommandApiController
    {
    }
}
