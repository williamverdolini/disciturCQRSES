using Discitur.Infrastructure.Api;
using Discitur.QueryStack.Model;
using System.Threading.Tasks;

namespace Discitur.QueryStack.Worker
{
    public interface IUserQueryWorker : IQueryWorker
    {
        Task<User> GetUserByUserName(string userName);
        Task<bool> IsAnyUserByUserName(string userName);
        Task<bool> IsAnyUserByEmail(string email);
        Task<UserActivation> GetUserActivationByUserName(string userName);
    }
}
