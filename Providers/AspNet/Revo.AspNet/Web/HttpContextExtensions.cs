using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Owin;

namespace Revo.AspNet.Web
{
    /// <summary>
    /// Provides extension methods for <see cref="HttpContext"/>.
    /// </summary>
    /// <remarks>Ported and extended from katana-clone/src/Microsoft.Owin.Host.SystemWeb/HttpContextExtensions.cs.</remarks>
    internal static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the <see cref="IOwinContext"/> for the current request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IOwinContext GetOwinContext(this HttpContext context)
        {
            IDictionary<string, object> environment = GetOwinEnvironment(context);

            if (environment == null)
            {
                throw new InvalidOperationException("No owin.Environment item was found in the context.");
            }

            return new OwinContext(environment);
        }

        /// <summary>
        /// Gets the <see cref="IOwinContext"/> for the current request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IOwinContext TryGetOwinContext(this HttpContext context)
        {
            IDictionary<string, object> environment = GetOwinEnvironment(context);

            if (environment == null)
            {
                return null;
            }

            return new OwinContext(environment);
        }

        /// <summary>
        /// Gets the <see cref="IOwinContext"/> for the current request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IOwinContext GetOwinContext(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            return request.RequestContext.HttpContext.GetOwinContext();
        }
        private static IDictionary<string, object> GetOwinEnvironment(this HttpContext context)
        {
            return (IDictionary<string, object>)context.Items[HttpContextItemKeys.OwinEnvironmentKey];
        }

        internal static class HttpContextItemKeys
        {
            public static readonly string OwinEnvironmentKey = "owin.Environment";
        }
    }
}
