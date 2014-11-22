using CommonDomain;
using System;

namespace Discitur.Infrastructure.Sagas
{
    public interface ISagaIdStore
    {
        Guid GetSagaIdFromCorrelationKey<TSaga>(object correlationKey) where TSaga : ISaga;
        bool ContainsSagaFromCorrelationKey<TSaga>(object correlationKey) where TSaga : ISaga;
        Guid AddSagaIdentifier<TSaga>(object correlationKey) where TSaga : ISaga;
    }

    internal class SagaIdentifier
    {
        public Guid SagaId { get; set; }
        public string SagaType { get; set; }
        public object CorrelationKey { get; set; }
    }
}
