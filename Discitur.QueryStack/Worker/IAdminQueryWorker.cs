using Discitur.Infrastructure.Api;

namespace Discitur.QueryStack.Worker
{
    public interface IAdminQueryWorker : IQueryWorker
    {
        void ClearReadModel();
    }
}
