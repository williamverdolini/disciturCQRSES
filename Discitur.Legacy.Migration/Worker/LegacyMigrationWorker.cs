﻿using Discitur.Infrastructure;
using Discitur.Legacy.Migration.Logic;
using System;
using System.Collections.Generic;

namespace Discitur.Legacy.Migration.Worker
{
    public class LegacyMigrationWorker
    {
        private readonly ILegacyMigrationManager _migrator;

        public LegacyMigrationWorker(ILegacyMigrationManager migrator)
        {
            Contract.Requires<ArgumentNullException>(migrator != null, "migrator");
            _migrator = migrator;
        }

        public IList<string> ExecuteMigration()
        {
            return _migrator.ExecuteMigration();
        }

    }
}
