namespace Discitur.Legacy.Migration.Infrastructure
{
    public interface IMigrationStepFactory
    {
        T Create<T>();
    }
}
