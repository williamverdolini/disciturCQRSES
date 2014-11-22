using CommonDomain.Core;
using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Messages.Events;
using System;

namespace Discitur.Domain.Saga
{
    /// <summary>
    /// Saga for User's Credits/Reputation
    /// </summary>
    public class UserCreditsSaga : SagaBase<Object>
    {
        public string LoginsNumber { get; private set; }
        public string ConsecutiveLoginsNumber { get; private set; }
        public Guid UserId { get; private set; }

        public UserCreditsSaga()
        {
            Register<LoggedInUserEvent>(Handle);
        }

        public static UserCreditsSaga CreateSaga(Guid id, Guid CorrelatedUserId)
        {
            var saga = new UserCreditsSaga();
            saga.Id = id.ToString();
            saga.UserId = CorrelatedUserId;
            return saga;
        }

        private void Handle(LoggedInUserEvent @event)
        {
            Dispatch(new AchieveAffecionatedUserBadgeCommand(@event.Id, @event.Date));
        }
    }
}
