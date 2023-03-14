using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Domain.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Projections;
using Revo.RavenDB.DataAccess;

namespace Revo.RavenDB.Projections
{
    public class RavenProjectionSubSystem : ProjectionSubSystem, IRavenProjectionSubSystem
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IRavenCrudRepository repository;

        public RavenProjectionSubSystem(IEntityTypeManager entityTypeManager,
            IEventMessageFactory eventMessageFactory, IServiceLocator serviceLocator,
            IRavenCrudRepository repository) : base(entityTypeManager, eventMessageFactory)
        {
            this.serviceLocator = serviceLocator;
            this.repository = repository;
        }

        protected override IEnumerable<IEntityEventProjector> GetProjectors(Type entityType, EventProjectionOptions options)
        {
            return serviceLocator.GetAll(
                    typeof(IRavenEntityEventProjector<>).MakeGenericType(entityType))
                .Cast<IEntityEventProjector>();
        }

        protected override async Task CommitUsedProjectorsAsync(IReadOnlyCollection<IEntityEventProjector> usedProjectors, EventProjectionOptions options)
        {
            await base.CommitUsedProjectorsAsync(usedProjectors, options);
            await repository.SaveChangesAsync();
        }
    }
}
