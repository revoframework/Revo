using System;

namespace Revo.Core.Commands.Lambda
{
    public class LambdaResultCommand : ICommand<object>
    {
        public LambdaResultCommand(Delegate @delegate, Type resultType)
        {
            Delegate = @delegate;
            ResultType = resultType;
        }

        public Delegate Delegate { get; }
        public Type ResultType { get; }
    }
}