namespace Discitur.Legacy.Migration.Infrastructure
{
    public interface IMigrationStep
    {
        void Execute();
    }
}
