using System.Threading.Tasks;

namespace GTRevo.Commands
{
    public interface ICommandFilter<in T>
        where T : ICommandBase
    {
        Task Handle(T command);
    }
}
