using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Revo.Core.Security;

namespace Revo.IntegrationTests.TestApp.Api
{
    /// <summary>
    /// Maps exceptions thrown when executing commands to HTTP status codes, like a real API would.
    /// </summary>
    public class ExceptionStatusCodeMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception e) when (!context.Response.HasStarted)
            {
                context.Response.StatusCode = e switch
                {
                    AuthorizationException => StatusCodes.Status403Forbidden,
                    ValidationException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };

                // returned for the tests to be able to show the cause of failures
                await context.Response.WriteAsync(e.ToString());
            }
        }
    }
}
