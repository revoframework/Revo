using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GTRevo.Core.Commands;

namespace GTRevo.Infrastructure.Validation
{
    public class CommandAttributeValidationFilter : IPreCommandFilter<ICommandBase>
    {
        public Task PreFilterAsync(ICommandBase command)
        {
            ValidationContext validationContext = new ValidationContext(command, null, null);
            Validator.ValidateObject(command, validationContext, true);

            return Task.FromResult(0);
        }
    }
}
