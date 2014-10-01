
namespace Discitur.Infrastructure.Commands
{
    public interface ICommandHandlerFactory
    {
        ICommandHandler<T>[] GetHandlersForCommand<T>(T command);
    }
}
