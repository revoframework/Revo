using System.Linq;
using Ninject;
using Ninject.Modules;
using Revo.AspNetCore.IO.OData;
using Revo.EFCore.AspNetCoreOData.Configuration;
using Revo.EFCore.AspNetCoreOData.IO.OData;
using Revo.EFCore.Configuration;

namespace Revo.EFCore.AspNetCoreOData
{
    public class EFCoreAspNetODataModule : NinjectModule
    {
        public override void Load()
        {
            var section = Kernel.GetBindings(typeof(EFCoreAspNetCoreODataConfigurationSection)).Any()
                ? Kernel.Get<EFCoreAspNetCoreODataConfigurationSection>()
                : null;
            
            Bind<IQueryableToODataResultConverter>()
                .ToMethod(ctx => new EFCoreQueryableToODataResultConverter(section?.DisableAsyncQueryableResolution ?? false))
                .InSingletonScope();
        }
    }
}
