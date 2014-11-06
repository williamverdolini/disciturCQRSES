using Discitur.QueryStack.Properties;

namespace Discitur.QueryStack
{
    public class AdminDatabase : IAdminDatabase
    {
        private DisciturContext Context;

        public AdminDatabase()
        {
            Context = new DisciturContext();
            Context.Database.Log = s => { System.Diagnostics.Debug.WriteLine(s); }; ; 
        }

        public void ClearAllTables()
        {
            Context.Database.ExecuteSqlCommand(Resources.ClearReadModel);
        }
    }

}
