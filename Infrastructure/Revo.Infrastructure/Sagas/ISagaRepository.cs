using Revo.Domain.Sagas;
using Revo.Infrastructure.EventSourcing;

namespace Revo.Infrastructure.Sagas
{
    public interface ISagaRepository : IEventSourcedRepository<ISaga>
    {
        ISagaMetadataRepository MetadataRepository { get; }
    }
}
