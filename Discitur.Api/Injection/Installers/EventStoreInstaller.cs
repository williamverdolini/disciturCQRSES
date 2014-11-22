﻿using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CommonDomain;
using CommonDomain.Core;
using CommonDomain.Persistence;
using CommonDomain.Persistence.EventStore;
using Discitur.Domain.Messages.Events;
using Discitur.Infrastructure;
using Discitur.Infrastructure.Events.Versioning;
using Discitur.Infrastructure.Sagas;
using NEventStore;
using NEventStore.Conversion;
using NEventStore.Dispatcher;
using NEventStore.Persistence.Sql.SqlDialects;
using System;
using System.Reflection;

namespace Discitur.Api.Injection.Installers
{
    //NOTE: for snapshots thanks to http://www.newdavesite.com/view/18365088
    public class AggregateFactory : IConstructAggregates
    {
        public IAggregate Build(Type type, Guid id, IMemento snapshot)
        {
            Type typeParam = snapshot != null ? snapshot.GetType() : typeof(Guid);
            object[] paramArray;
            if (snapshot != null)
                paramArray = new object[] { snapshot };
            else
                paramArray = new object[] { id };

            ConstructorInfo constructor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeParam }, null);

            if (constructor == null)
            {
                throw new InvalidOperationException(
                    string.Format("Aggregate {0} cannot be created: constructor with proper parameter not provided",
                                  type.Name));
            }
            return constructor.Invoke(paramArray) as IAggregate;
        }
    }

    public class EventStoreInstaller : IWindsorInstaller
    {
        private static IStoreEvents _store;

        //private void SubscribeEvents(IBus bus)
        //{
        //    bus.Subscribe<ToDoEventHandlers>();
        //}

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IBus, IDispatchCommits>().ImplementedBy<InMemoryBus>().LifestyleSingleton());
            container.Register(
                Classes
                .FromAssemblyContaining<SavedNewDraftLessonEvent>()
                .BasedOn(typeof(IUpconvertEvents<,>)) // That implement IUpconvertEvents<,> Interface
                .WithService.Base()
                .LifestyleTransient());

            _store =
                    Wireup
                    .Init()
                    .LogToOutputWindow()
                    .UsingInMemoryPersistence()
                    .UsingSqlPersistence("EventStore") // Connection string is in web.config
                        .WithDialect(new MsSqlDialect())
                        .InitializeStorageEngine()
                        //.UsingJsonSerialization()
                        .UsingNewtonsoftJsonSerialization(new VersionedEventSerializationBinder())
                        .Compress()
                    .UsingSynchronousDispatchScheduler()
                        .DispatchTo(container.Resolve<IDispatchCommits>())
                        .Startup(DispatcherSchedulerStartup.Explicit)
                    .UsingEventUpconversion()
                        .WithConvertersFromAssemblyContaining(new Type[] { typeof(SavedNewDraftLessonEvent) })
                    .Build();

            _store.StartDispatchScheduler();

            container.Register(
                Component.For<IStoreEvents>().Instance(_store),
                Component.For<IRepository>().ImplementedBy<EventStoreRepository>().LifeStyle.Transient,
                Component.For<ISagaRepository>().ImplementedBy<SagaEventStoreRepository>().LifeStyle.Transient,
                Component.For<ISagaIdStore>().ImplementedBy<InMemorySagaIdStore>().LifeStyle.Transient,
                Component.For<IConstructAggregates>().ImplementedBy<AggregateFactory>().LifeStyle.Transient,
                Component.For<IDetectConflicts>().ImplementedBy<ConflictDetector>().LifeStyle.Transient);

            //// Elegant way to write the same Registration as before:
            //container.Register(
            //    Component.For<IStoreEvents>().Instance(_store),
            //    C<IRepository, EventStoreRepository>(),
            //    C<IConstructAggregates, AggregateFactory>(),
            //    C<IDetectConflicts, ConflictDetector>());		            
        }

        private static ComponentRegistration<TS> C<TS, TC>()
            where TC : TS
            where TS : class
        {
            return Component.For<TS>().ImplementedBy<TC>().LifeStyle.Transient;
        }

    }
}