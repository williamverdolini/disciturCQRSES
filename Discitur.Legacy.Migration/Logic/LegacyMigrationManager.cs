using Discitur.Infrastructure;
using Discitur.Legacy.Migration.Infrastructure;
using Discitur.Legacy.Migration.Model;
using System;
using System.Collections.Generic;

namespace Discitur.Legacy.Migration.Logic
{
    public class LegacyMigrationManager : ILegacyMigrationManager
    {
        private readonly IMigrationStepFactory _migrationStepFactory;
        private IList<string> _migrationLogs;

        public LegacyMigrationManager(IMigrationStepFactory migrationStepFactory)
        {
            Contract.Requires<ArgumentNullException>(migrationStepFactory != null, "migrationStepFactory");
            _migrationStepFactory = migrationStepFactory;
            _migrationLogs = new List<string>();
        }

        public IList<string> ExecuteMigration()
        {
            // Configure Migration Process with sequence of the migration steps
            return MigrationProcess
                .Init(_migrationLogs)                                                     // Initialize Migration Process
                .Then(_migrationStepFactory.Create<IUserMigration>())       // Migrate Users
                .Then(_migrationStepFactory.Create<IQueryIdsMigration>())   // Migrate read-model IDs
                .Then(_migrationStepFactory.Create<ILessonMigration>())     // Migrate Lessons
                .Configured()                                               // Migration Process Configured
                .Execute();

            //return _migrationLogs;

        }

    }
}
