using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.EventSourcing;

namespace GTRevo.Infrastructure.Sagas
{
    public interface ISagaRepository : IEventSourcedRepository<ISaga>
    {
        ISagaMetadataRepository MetadataRepository { get; }
    }
}
