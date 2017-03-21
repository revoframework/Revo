using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace GTRevo.Platform.Web
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ValidateHttpAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (actionContext.Request.Method.IsSafe())
            {
                return;
            }

            var headers = actionContext.Request.Headers;

            // TODO alternative authentication for external API clients that don't use sessions
            try
            {
                IEnumerable<string> tokens;
                if (headers.TryGetValues(AntiForgeryConsts.HeaderTokenName, out tokens))
                {
                    string headerToken = tokens.First();

                    CookieState cookie = headers.GetCookies().Select(c => c[AntiForgeryConsts.CookieTokenName]).FirstOrDefault();
                    string cookieToken = cookie?.Value;

                    if (cookieToken?.Length > 0)
                    {
                        AntiForgery.Validate(cookieToken, headerToken);
                    }
                    else
                    {
                        actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "No CSRF token value supplied in request cookies");
                    }
                }
                else
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "No CSRF token value supplied in request headers");
                }
            }
            catch (HttpAntiForgeryException)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid CSRF token");
            }

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            string csrfCookie, csrfFormValue;
            AntiForgery.GetTokens(null, out csrfCookie, out csrfFormValue);

            actionExecutedContext.Response.Headers.AddCookies(new List<CookieHeaderValue>()
            {
                new CookieHeaderValue(AntiForgeryConsts.CookieTokenName, csrfCookie)
                {
                    Expires = DateTimeOffset.Now.AddDays(1),
                    Domain = actionExecutedContext.Request.RequestUri.Host,
                    Path = "/",
                    HttpOnly = true
                },
                new CookieHeaderValue(AntiForgeryConsts.CookieFormTokenName, csrfFormValue)
                {
                    Expires = DateTimeOffset.Now.AddDays(1),
                    Domain = actionExecutedContext.Request.RequestUri.Host,
                    Path = "/",
                    HttpOnly = false
                }
            });
        }
    }
}
