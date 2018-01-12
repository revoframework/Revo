using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events
{
    public interface IEventMessageFactory
    {
        Task<IEventMessageDraft> CreateMessageAsync(IEvent @event);
    }
}