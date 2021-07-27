using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Core;

namespace Revo.Core.Commands.Lambda
{
    public class LambdaQueryHandler : IQueryHandler<LambdaQuery, object>
    {
        private readonly IServiceLocator serviceLocator;

        public LambdaQueryHandler(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public async Task<object> HandleAsync(LambdaQuery query, CancellationToken cancellationToken)
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