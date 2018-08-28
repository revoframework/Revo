using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Configuration;

namespace Revo.DataAccess.EF6
{
    public static class EF6DataAccessConfigurationExtensions
    {
        public static IRevoConfiguration UseEF6DataAccess(this IRevoConfiguration configuration,
            bool? useAsPrimaryRepository = true,
            EF6ConnectionConfiguration connection = null,
            Action<EF6DataAccessConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<EF6DataAccessConfigurationSection>();
            section.IsActive = true;
            section.Connection = connection ?? section.Connection;
            section.UseAsPrimaryRepository = useAsPrimaryRepository ?? section.UseAsPrimaryRepository;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    c.LoadModule(new EF6DataAccessModule(section.Connection, section.UseAsPrimaryRepository));

                    if (section.ConventionTypes != null)
                    {
                        foreach (var conventionType in section.ConventionTypes)
                        {
                            c.Kernel.Bind<IConvention>().To(conventionType);
                        }
                    }
                }
            });

            return configuration;
        }
    }
}
