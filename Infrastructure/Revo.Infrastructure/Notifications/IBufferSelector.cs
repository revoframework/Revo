using System;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Notifications
{
    public interface IBufferSelector<in T> where T : INotification
    {
        Task<Guid> SelectBufferIdAsync(T notification);
    }
}
