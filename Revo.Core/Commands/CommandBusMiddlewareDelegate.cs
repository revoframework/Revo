using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    public delegate Task<object> CommandBusMiddlewareDelegate(ICommandBase command);
}