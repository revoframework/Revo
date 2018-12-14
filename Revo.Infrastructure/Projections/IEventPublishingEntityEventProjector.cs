using Revo.Core.Events;
using Revo.Infrastructure.Events;

namespace Revo.Infrastructure.Projections
{
    public interface IEventPublishingEntityEventProjector : IEntityEventProjector
    {
        IPublishEventBuffer EventBuffer { get; set; }
        IEventMessageFactory EventMessageFactory { get; set; }
    }
}