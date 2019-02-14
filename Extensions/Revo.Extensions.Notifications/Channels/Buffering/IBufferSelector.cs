using System;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public interface IBufferSelector<in T> where T : INotification
    {
        Task<Guid> SelectBufferIdAsync(T notification);
    }
}
