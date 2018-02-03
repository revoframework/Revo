using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    public interface IPreCommandFilter<in T>
        where T : ICommandBase
    {
        Task PreFilterAsync(T command);
    }
}
