using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Core;

namespace Revo.Core.Commands.Lambda
{
    public class LambdaCommandHandler :
        ICommandHandler<LambdaCommand>,
        ICommandHandler<LambdaResultCommand, object>
    {
        private readonly IServiceLocator serviceLocator;

        public LambdaCommandHandler(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public async Task HandleAsync(LambdaCommand command, CancellationToken cancellationToken)
        {
            var lambdaParams = command.Delegate.Method.GetParameters()
                .Select(x => serviceLocator.Get(x.ParameterType))
                .ToArray();
            
            var result = command.Delegate.DynamicInvoke(lambdaParams);
            if (result is Task resultTask)
            {
                await resultTask;
            }
        }

        public async Task<object> HandleAsync(LambdaResultCommand query, CancellationToken cancellationToken)
        {
            var lambdaParams = query.Delegate.Method.GetParameters()
                .Select(x => serviceLocator.Get(x.ParameterType))
                .ToArray();

            var result = query.Delegate.DynamicInvoke(lambdaParams);
            if (result is Task resultTask)
            {
                await resultTask;
                var resultValue = resultTask.GetType().GetProperty(nameof(Task<object>.Result)).GetValue(resultTask);
                return resultValue;
            }
            else
            {
                return result;
            }
        }
    }
}