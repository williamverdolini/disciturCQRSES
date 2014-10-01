using FluentValidation;

namespace Discitur.Infrastructure.Commands
{
    public interface ICommandValidatorFactory
    {
        IValidator<T>[] GetValidatorsForCommand<T>(T command);        
    }
}
