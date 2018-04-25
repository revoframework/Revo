using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Revo.Core.Commands;

namespace Revo.Examples.HelloAspNet.Bootstrap.Messages.Commands
{
    public class AddTodoCommand : ICommand
    {
        public AddTodoCommand(string title)
        {
            Title = title;
        }

        public string Title { get; private set; }
    }
}