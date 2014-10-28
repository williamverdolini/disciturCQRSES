using Discitur.Infrastructure.Events.Versioning;
using NEventStore;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Discitur.Infrastructure
{
    public static class NEventStoreExtensions
    {
        private const string AggregateTypeHeader = "AggregateType";

        public static SerializationWireup UsingNewtonsoftJsonSerialization(this PersistenceWireup wireup, SerializationBinder binder, params Type[] knownTypes)
        {
            return wireup.UsingCustomSerialization(
                new NewtonsoftJsonSerializer(binder, new JsonConverter[] { }, knownTypes)
                    );
        }

        // Header of stream (like NEeventStore does). See: https://github.com/NEventStore/NEventStore/blob/master/src/NEventStore/CommonDomain/Persistence/EventStore/EventStoreRepository.cs#L197
        public static void AddMigrationCommit(this IEventStream stream, Type mementoType, object @event)
        {
            stream.UncommittedHeaders[AggregateTypeHeader] = mementoType.FullName.Replace("Memento", "");
            stream.Add(new EventMessage { Body = @event });
            stream.CommitChanges(Guid.NewGuid());
        }
    }
}
