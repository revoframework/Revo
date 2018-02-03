using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    public interface IPostCommandFilter<in T>
        where T : ICommandBase
    {
        Task PostFilterAsync(T command, object result);
    }
}
