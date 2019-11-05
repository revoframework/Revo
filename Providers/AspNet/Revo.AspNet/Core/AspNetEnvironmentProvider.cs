using System.Web;
using Revo.Core.Core;

namespace Revo.AspNet.Core
{
    public class AspNetEnvironmentProvider : IEnvironmentProvider
    {
        public AspNetEnvironmentProvider()
        {
            IsDevelopment = HttpContext.Current.IsDebuggingEnabled;
        }
        public bool? IsDevelopment { get; }
    }
}