using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public interface IBufferSelector<in T> where T : INotification
    {
        Task<string> SelectBufferIdAsync(T notification);
    }
}
