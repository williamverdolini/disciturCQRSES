using System.Collections.Generic;

namespace Discitur.Legacy.Migration.Infrastructure
{
    public interface IMigrationStep
    {
        IList<string> Execute();
    }
}
