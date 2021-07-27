using System;

namespace Revo.Core.Commands.Lambda
{
    public class LambdaCommand : ICommand
    {
        public LambdaCommand(Delegate @delegate)
        {
            Delegate = @delegate;
        }

        public Delegate Delegate { get; }
    }
}