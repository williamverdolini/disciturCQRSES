using Discitur.Infrastructure;
using Discitur.Legacy.Migration.Infrastructure;
using Discitur.Legacy.Migration.Model;
using System;

namespace Discitur.Legacy.Migration.Logic
{
    public class LegacyMigrationManager : ILegacyMigrationManager
    {
        private readonly IMigrationStepFactory _migrationStepFactory;

        public LegacyMigrationManager(IMigrationStepFactory migrationStepFactory)
        {
            Contract.Requires<ArgumentNullException>(migrationStepFactory != null, "migrationStepFactory");
            _migrationStepFactory = migrationStepFactory;
        }

        public void ExecuteMigration()
        {
            // Configure Migration Process with sequence of the migration steps
            MigrationProcess
                .Init()                                                     // Initialize Migration Process
                .Then(_migrationStepFactory.Create<IUserMigration>())       // Migrate Users
                .Then(_migrationStepFactory.Create<IQueryIdsMigration>())   // Migrate read-model IDs
                .Then(_migrationStepFactory.Create<ILessonMigration>())     // Migrate Lessons
                .Configured()                                               // Migration Process Configured
                .Execute();

        }

    }
}
