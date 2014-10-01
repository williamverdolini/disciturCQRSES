
namespace Discitur.Infrastructure.Events
{
    public interface IEventHandler<T>
    {
        void Handle(T @event);
    }
}
