using System;
using System.Linq;
using Revo.Core.Configuration;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Jobs.InMemory;
using Revo.Infrastructure.Sagas;
using Revo.Infrastructure.Tenancy;

namespace Revo.Infrastructure
{
    public static class InfrastructureConfigurationExtensions
    {
        public static IRevoConfiguration ConfigureInfrastructure(this IRevoConfiguration configuration,
            Action<InfrastructureConfigurationSection> action = null)
        {
            configuration.ConfigureCore();
            configuration.UseInMemoryJobs(isActive: null); // activate only if not previously disabled

            var section = configuration.GetSection<InfrastructureConfigurationSection>();

            action?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (!c.Kernel.GetBindings(typeof(IAsyncEventPipelineConfiguration)).Any())
                {
                    c.Kernel.Bind<IAsyncEventPipelineConfiguration>().ToConstant(section.AsyncEventPipeline);
                }
                
                if (!c.Kernel.GetBindings(typeof(DatabaseMigrationsConfiguration)).Any())
                {
                    c.Kernel.Bind<DatabaseMigrationsConfiguration>().ToConstant(section.DatabaseMigrations);
                }

                c.LoadModule(new DatabaseMigrationsModule(section.DatabaseMigrations));
                c.LoadModule(new TenancyModule(section.Tenancy));

                if (section.EnableSagas)
                {
                    c.LoadModule<SagasModule>();
                }
            });

            return configuration;
        }
    }
}
