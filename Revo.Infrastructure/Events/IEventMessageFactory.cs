using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events
{
    public interface IEventMessageFactory
    {
        Task<IEventMessageDraft> CreateMessageAsync(IEvent @event);
    }
}