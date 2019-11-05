using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.ContextPreservation;
using Revo.Core.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.Domain;

namespace Revo.EFCore.DataAccess.Configuration
{
    public class EFCoreDataAccessConfigurationSection : IRevoConfigurationSection
    {
        private readonly List<Func<IContext, IEFCoreConvention>> conventions =
            new List<Func<IContext, IEFCoreConvention>>();

        public EFCoreDataAccessConfigurationSection()
        {
            AddConvention<ScanForEntitiesConvention>(-100);
            AddConvention<ReadModelConvention>(0);
            AddConvention<BasicDomainModelConvention>(100);
        }

        public bool IsActive { get; set; }
        public bool UseAsPrimaryRepository { get; set; }
        public bool EnableMigrationProvider { get; set; } = true;
        public IReadOnlyCollection<Func<IContext, IEFCoreConvention>> Conventions => conventions;
        public Action<DbContextOptionsBuilder> Configurer { get; set; }

        public EFCoreDataAccessConfigurationSection AddConvention(IEFCoreConvention convention, int? order = null)
        {
            conventions.Add(context =>
            {
                if (order.HasValue)
                {
                    convention.Order = order.Value;
                }

                return convention;
            });

            return this;
        }

        public EFCoreDataAccessConfigurationSection AddConvention<T>(int? order = null) where T : IEFCoreConvention
        {
            conventions.Add(context =>
            {
                var convention = context.ContextPreservingGet<T>();
                if (order.HasValue)
                {
                   convention.Order = order.Value;
                }

                return convention;
            });

            return this;
        }

        public EFCoreDataAccessConfigurationSection AddConvention(Func<IContext, IEFCoreConvention> conventionFunc)
        {
            conventions.Add(conventionFunc);
            return this;
        }

        public EFCoreDataAccessConfigurationSection ClearConventions()
        {
            conventions.Clear();
            return this;
        }

        public EFCoreDataAccessConfigurationSection RemoveConvention(int index)
        {
            conventions.RemoveAt(index);
            return this;
        }
    }
}
