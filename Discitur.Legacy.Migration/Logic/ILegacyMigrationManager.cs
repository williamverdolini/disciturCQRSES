using System.Collections.Generic;

namespace Discitur.Legacy.Migration.Logic
{
    public interface ILegacyMigrationManager
    {
        IList<string> ExecuteMigration();
    }
}
