using Discitur.Infrastructure;
using Discitur.QueryStack.Model;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Discitur.QueryStack.Worker
{
    public class UserQueryWorker : IUserQueryWorker
    {
        private readonly IDatabase _database;

        public UserQueryWorker(IDatabase database)
        {
            Contract.Requires<ArgumentNullException>(database != null, "database");
            _database = database;
        }

        public async Task<User> GetUserByUserName(string userName)
        {
            return await _database.Users.Where(u => u.UserName.Equals(userName)).FirstAsync<User>();
        }

        public async Task<bool> IsAnyUserByUserName(string userName)
        {
            return await _database.Users.AnyAsync(u => u.UserName.Equals(userName));
        }

        public async Task<bool> IsAnyUserByEmail(string email)
        {
            return await _database.Users.AnyAsync(u => u.Email.Equals(email));
        }

        public async Task<UserActivation> GetUserActivationByUserName(string userName)
        {
            return await _database.UserActivations.Where(u => u.UserName.Equals(userName)).FirstAsync<UserActivation>();
        }
    }
}
