using System.Threading.Tasks;

namespace GTRevo.Core.Commands
{
    public interface ICommandFilter<in T>
        where T : ICommandBase
    {
        Task Handle(T command);
    }
}
