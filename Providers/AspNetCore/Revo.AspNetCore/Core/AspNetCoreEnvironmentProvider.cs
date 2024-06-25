using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Revo.Core.Core;

namespace Revo.AspNetCore.Core
{
    public class AspNetCoreEnvironmentProvider : IEnvironmentProvider
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public AspNetCoreEnvironmentProvider(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        public bool? IsDevelopment => webHostEnvironment.IsDevelopment();
    }
}