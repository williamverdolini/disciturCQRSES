using Discitur.CommandStack.ViewModel;
using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Model;
using Discitur.Infrastructure;
using Discitur.QueryStack;
using System;

namespace Discitur.CommandStack.Worker
{
    public class UserCommandWorker : IUserCommandWorker
    {
        private readonly IBus bus;
        private readonly IDatabase database;

        public UserCommandWorker(IBus commandBus, IDatabase db)
        {
            Contract.Requires<ArgumentNullException>(commandBus != null, "commandBus");
            Contract.Requires<ArgumentNullException>(db != null, "db");

            bus = commandBus;
            database = db;
        }

        #region Command Responsibility
        public void RegisterUser(RegisterUserViewModel model)
        {
            RegisterUserCommand command = new RegisterUserCommand(model.Name, model.Surname, model.Email, model.UserName);
            bus.Send<RegisterUserCommand>(command);
            model.ActivationKey = command.Id.ToString();
            //string _newId = database.IdMaps.GetModelId<User>(command.Id).ToString();

        }

        public void ActivateUser(ActivateUserViewModel model)
        {
            bus.Send<ActivateUserCommand>(new ActivateUserCommand(model.UserName, model.Key));
        }

        public void ChangeUserEmail(ChangeUserEmailViewModel model)
        {
            Guid userId = database.IdMaps.GetAggregateId<User>(model.UserId);
            bus.Send<ChangeUserEmailCommand>(new ChangeUserEmailCommand(userId, model.Email, model.UserName));
        }

        public void ChangeUserPicture(ChangeUserPictureViewModel model)
        {
            Guid userId = database.IdMaps.GetAggregateId<User>(model.UserId);
            bus.Send<ChangeUserPictureCommand>(new ChangeUserPictureCommand(userId, model.Picture));
        }

        #endregion
    }
}
