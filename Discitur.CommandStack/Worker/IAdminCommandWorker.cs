using Discitur.Infrastructure.Api;

namespace Discitur.CommandStack.Worker
{
    public interface IAdminCommandWorker : ICommandWorker
    {
        void ReplayAllEvents();
    }
}
