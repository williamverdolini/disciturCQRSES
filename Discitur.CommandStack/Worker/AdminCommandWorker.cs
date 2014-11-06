using Discitur.Infrastructure;
using Discitur.Infrastructure.Events.Replaying;
using System;

namespace Discitur.CommandStack.Worker
{
    public class AdminCommandWorker : IAdminCommandWorker
    {
        private readonly IEventsReplayer Replayer;

        public AdminCommandWorker(IEventsReplayer replayer)
        {
            Contract.Requires<System.ArgumentNullException>(replayer != null, "replayer");
            Replayer = replayer;
        }

        public void ReplayAllEvents()
        {
            Replayer.ReplayAllEvents();
        }
    }
}
