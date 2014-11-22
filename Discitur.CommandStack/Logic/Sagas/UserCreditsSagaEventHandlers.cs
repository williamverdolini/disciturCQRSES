using CommonDomain.Persistence;
using Discitur.Domain.Messages.Events;
using Discitur.Domain.Saga;
using Discitur.Infrastructure;
using Discitur.Infrastructure.Events;
using Discitur.Infrastructure.Sagas;
using System;

namespace Discitur.CommandStack.Logic.Sagas
{
    public class UserCreditsSagaEventHandlers : 
        IEventHandler<LoggedInUserEvent>
    {
        private readonly ISagaRepository SagaRepo;
        private readonly ISagaIdStore SagaIdStore;

        public UserCreditsSagaEventHandlers(ISagaRepository sagaRepository, ISagaIdStore sagaIdStore)
        {
            Contract.Requires<ArgumentNullException>(sagaRepository != null, "sagaRepository");
            Contract.Requires<ArgumentNullException>(sagaIdStore != null, "sagaIdStore");
            SagaRepo = sagaRepository;
            SagaIdStore = sagaIdStore;
        }

        public void Handle(LoggedInUserEvent @event)
        {
            Guid sagaid = SagaIdStore.GetSagaIdFromCorrelationKey<UserCreditsSaga>(@event.Id);

            UserCreditsSaga saga;
            if (sagaid.Equals(Guid.Empty))
            {
                sagaid = SagaIdStore.AddSagaIdentifier<UserCreditsSaga>(@event.Id);
                saga = UserCreditsSaga.CreateSaga(id: sagaid, CorrelatedUserId: @event.Id);
            }
            else
            {
                saga = SagaRepo.GetById<UserCreditsSaga>(sagaid);
            }

            saga.Transition(@event);
            SagaRepo.Save(saga, Guid.NewGuid(), null);
        }

    }
}
