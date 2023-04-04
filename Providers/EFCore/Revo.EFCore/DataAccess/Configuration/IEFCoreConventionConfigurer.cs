using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Configuration
{
    public interface IEFCoreConventionConfigurer
    {
        void ConfigureConventions(ModelConfigurationBuilder configurationBuilder);
    }
}