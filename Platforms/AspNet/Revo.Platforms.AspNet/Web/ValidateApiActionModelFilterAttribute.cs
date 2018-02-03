using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Revo.Platforms.AspNet.Web
{
    public class ValidateApiActionModelFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var missingArgument = actionContext.ActionArguments.FirstOrDefault(kv => kv.Value == null); // TODO allow for nullable value types?
            if (missingArgument.Key != null) 
            { 
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    $"Service method parameter cannot be null: {missingArgument.Key}");
            }
            else if (!actionContext.ModelState.IsValid)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState);
            }
        }
    }
}
