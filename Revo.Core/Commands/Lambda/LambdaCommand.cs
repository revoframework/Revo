using System;

namespace Revo.Core.Commands.Lambda
{
    public class LambdaCommand(Delegate @delegate) : ICommand
    {
        public Delegate Delegate { get; } = @delegate;
    }
}