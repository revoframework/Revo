using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GTRevo.Core.Commands;

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
