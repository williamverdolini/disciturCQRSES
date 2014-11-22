using CommonDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.Infrastructure.Sagas
{
    // thanks to rucka. https://github.com/rucka/RealWorldCqrs/blob/master/ManagedDesigns.RealWorldCqrs.Core/Infrastructure/ISagaIdStore.cs
    public class InMemorySagaIdStore : ISagaIdStore
    {
        private static readonly IList<SagaIdentifier> Data = new List<SagaIdentifier>();

        //TODO: SRP violated! divide Get from insert SagaIdentifier
        public Guid GetSagaIdFromCorrelationKey<TSaga>(object correlationKey) where TSaga : ISaga
        {
            if (correlationKey == null) throw new ArgumentNullException("correlationKey");
            lock (Data)
            {
                Guid sagaid = Guid.Empty;
                SagaIdentifier[] data = Data.ToArray();
                if (data.Any(i => i.SagaType == typeof(TSaga).FullName && correlationKey.Equals(i.CorrelationKey)))
                {
                    sagaid =
                        data.Single(
                            i => i.SagaType == typeof(TSaga).FullName && correlationKey.Equals(i.CorrelationKey))
                            .SagaId;
                }
                //else
                //{
                //    sagaid = Guid.NewGuid();
                //    Data.Add(new SagaIdentifier
                //    {
                //        SagaId = sagaid,
                //        SagaType = typeof(TSaga).FullName,
                //        CorrelationKey = correlationKey
                //    });
                //}
                return sagaid;
            }
        }

        public Guid AddSagaIdentifier<TSaga>(object correlationKey) where TSaga : ISaga
        {
            Contract.Requires<ArgumentNullException>(correlationKey != null, "correlationKey");

            lock (Data)
            {
                Guid sagaid = Guid.NewGuid();
                Data.Add(new SagaIdentifier
                {
                    SagaId = sagaid,
                    SagaType = typeof(TSaga).FullName,
                    CorrelationKey = correlationKey
                });
                return sagaid;
            }
        }

        public bool ContainsSagaFromCorrelationKey<TSaga>(object correlationKey) where TSaga : ISaga
        {
            Contract.Requires<ArgumentNullException>(correlationKey != null, "correlationKey");
            SagaIdentifier[] data = Data.ToArray();
            return data.Any(i => i.SagaType == typeof(TSaga).FullName && correlationKey.Equals(i.CorrelationKey));
        }
    }
}
