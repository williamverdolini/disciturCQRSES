namespace Discitur.Infrastructure.Events.Replaying
{
    public interface IEventsReplayer
    {
        void ReplayAllEvents();
    }
}
