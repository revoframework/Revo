using System;

namespace Revo.Core.Commands.Lambda
{
    public class LambdaQuery : IQuery<object>
    {
        public LambdaQuery(Delegate @delegate, Type resultType)
        {
            Delegate = @delegate;
            ResultType = resultType;
        }

        public Delegate Delegate { get; }
        public Type ResultType { get; }
    }
}