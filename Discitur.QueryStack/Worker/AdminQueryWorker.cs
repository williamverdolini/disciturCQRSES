using Discitur.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.QueryStack.Worker
{
    public class AdminQueryWorker : IAdminQueryWorker
    {
        private readonly IAdminDatabase _database;

        public AdminQueryWorker(IAdminDatabase database)
        {
            Contract.Requires<ArgumentNullException>(database != null, "database");
            _database = database;
        }

        public void ClearReadModel()
        {
            _database.ClearAllTables();
        }
    }
}
