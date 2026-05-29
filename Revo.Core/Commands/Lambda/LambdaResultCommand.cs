using System;

namespace Revo.Core.Commands.Lambda
{
    public class LambdaResultCommand(Delegate @delegate, Type resultType) : ICommand<object>
    {
        public Delegate Delegate { get; } = @delegate;
        public Type ResultType { get; } = resultType;
    }
}