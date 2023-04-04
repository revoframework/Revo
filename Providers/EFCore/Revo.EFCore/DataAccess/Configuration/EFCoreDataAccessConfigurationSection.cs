using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.Domain;

namespace Revo.EFCore.DataAccess.Configuration
{
    public class EFCoreDataAccessConfigurationSection : IRevoConfigurationSection
    {
        private readonly List<Func<IServiceProvider, IEFCoreConvention>> conventions =
            new List<Func<IServiceProvider, IEFCoreConvention>>();

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

        public IReadOnlyCollection<Func<IServiceProvider, IEFCoreConvention>> Conventions => conventions;
        public Action<DbContextOptionsBuilder> Configurer { get; set; }

        public EFCoreDataAccessConfigurationSection AddConvention(IEFCoreConvention convention, int? order = null)
        {
            conventions.Add(sp =>
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
            conventions.Add(sp =>
            {
                var convention = sp.GetService(typeof(T)) as IEFCoreConvention
                    ?? (IEFCoreConvention)ActivatorUtilities.CreateInstance(sp, typeof(T));
                if (order.HasValue)
                {
                   convention.Order = order.Value;
                }

                return convention;
            });

            return this;
        }

        public EFCoreDataAccessConfigurationSection AddConvention(Func<IServiceProvider, IEFCoreConvention> conventionFunc)
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
