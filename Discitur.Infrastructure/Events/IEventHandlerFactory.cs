
namespace Discitur.Infrastructure.Events
{
    public interface IEventHandlerFactory
    {
        IEventHandler<T>[] GetHandlersForEvent<T>(T @event);
    }
}
