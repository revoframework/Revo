using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Platform.Commands;

namespace GTRevo.Infrastructure.Validation
{
    public class CommandAttributeValidationFilter : ICommandFilter<ICommandBase>
    {
        public Task Handle(ICommandBase command)
        {
            ValidationContext validationContext = new ValidationContext(command, null, null);
            Validator.ValidateObject(command, validationContext, true);

            return Task.FromResult(0);
        }
    }
}
