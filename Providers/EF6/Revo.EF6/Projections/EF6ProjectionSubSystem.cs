using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Domain.Entities;
using Revo.EF6.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Projections;

namespace Revo.EF6.Projections
{
    public class EF6ProjectionSubSystem : ProjectionSubSystem, IEF6ProjectionSubSystem
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IEF6CrudRepository crudRepository;

        public EF6ProjectionSubSystem(IEntityTypeManager entityTypeManager, IEventMessageFactory eventMessageFactory,
            IServiceLocator serviceLocator, IEF6CrudRepository crudRepository) : base(entityTypeManager, eventMessageFactory)
        {
            this.serviceLocator = serviceLocator;
            this.crudRepository = crudRepository;
        }

        protected override IEnumerable<IEntityEventProjector> GetProjectors(Type entityType, EventProjectionOptions options)
        {
            return serviceLocator.GetAll(
                    typeof(IEF6EntityEventProjector<>).MakeGenericType(entityType))
                .Cast<IEntityEventProjector>();
        }

        protected override async Task CommitUsedProjectorsAsync(IReadOnlyCollection<IEntityEventProjector> usedProjectors, EventProjectionOptions options)
        {
            await base.CommitUsedProjectorsAsync(usedProjectors, options);
            await crudRepository.SaveChangesAsync();
        }
    }
}
