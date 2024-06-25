using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Revo.Core.Core;

namespace Revo.AspNetCore.Core
{
    public class AspNetCoreEnvironmentProvider(IWebHostEnvironment webHostEnvironment) : IEnvironmentProvider
    {
        public bool? IsDevelopment => webHostEnvironment.IsDevelopment();
    }
}