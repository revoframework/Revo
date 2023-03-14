using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// This registers a custom EntityQueryProvider which enables certain Revo features.
        /// This is required to use e.g. <see cref="Security.AuthorizationQueryableExtensions"/>.
        /// </summary>
        public bool EnableCustomQueryProvider { get; set; } = true;

        /// <summary>
        /// This registers a custom IQueryTranslationPreprocessor implementation which makes it possible
        /// to register own <see cref="Query.IQueryTranslationPlugin"/>.
        /// </summary>
        public bool EnableCustomQueryTranslationPreprocessor { get; set; } = true;

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
