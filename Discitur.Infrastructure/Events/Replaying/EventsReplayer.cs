using NEventStore;
using System;
using System.Linq;

namespace Discitur.Infrastructure.Events.Replaying
{
    public class EventsReplayer : IEventsReplayer
    {
        private readonly IStoreEvents _store;
        private readonly IBus _bus;

        public EventsReplayer(IStoreEvents store, IBus bus)
        {
            Contract.Requires<ArgumentNullException>(store != null, "store");
            Contract.Requires<ArgumentNullException>(bus != null, "bus");
            _store = store;
            _bus = bus;
        }

        public void ReplayAllEvents()
        {
            var commits = _store.Advanced.GetFrom(null).ToArray();

            foreach (var commit in commits)
            {
                var evts = commit.Events
                    .Where(x => x.Body is Event)
                    .Select(evt => (dynamic)evt.Body)
                    .FirstOrDefault();

                _bus.Publish(evts);
            }
        }
    }
}
