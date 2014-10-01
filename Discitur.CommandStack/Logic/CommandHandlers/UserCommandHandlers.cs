using CommonDomain.Persistence;
using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Model;
using Discitur.Infrastructure;
using Discitur.Infrastructure.Commands;
using NEventStore;
using System;

namespace Discitur.CommandStack.Logic.CommandHandlers
{
    public class UserCommandHandlers : 
        ICommandHandler<RegisterUserCommand>,
        ICommandHandler<ActivateUserCommand>,
        ICommandHandler<ChangeUserEmailCommand>,
        ICommandHandler<ChangeUserPictureCommand>
    {
        // Repository to get/save Aggregates/Entities from/to Domain Model
        private readonly IRepository repo;
        // Event Store to Raise (Extra-Domain) Events
        private readonly IStoreEvents store;

        public UserCommandHandlers(IRepository repository, IStoreEvents eventStore) 
            //TODO: for Snapshooting ctor
            //: base(repository, eventStore)
        {
            //Guard clauses
            Contract.Requires<ArgumentNullException>(repository != null, "repository");
            Contract.Requires<ArgumentNullException>(eventStore != null, "eventStore");
            repo = repository;
            store = eventStore;
        }

        public void Handle(RegisterUserCommand command)
        {
            User user = new User(command.Id, command.Name, command.Surname, command.Email, command.UserName);
            repo.Save(user, Guid.NewGuid());
        }

        public void Handle(ActivateUserCommand command)
        {
            User user = repo.GetById<User>(new Guid(command.Key));
            user.Activate();
            repo.Save(user, Guid.NewGuid());
        }

        public void Handle(ChangeUserEmailCommand command)
        {
            User user = repo.GetById<User>(command.Id);
            user.ChangeEmail(command.Email);
            repo.Save(user, Guid.NewGuid());
        }

        public void Handle(ChangeUserPictureCommand command)
        {
            User user = repo.GetById<User>(command.Id);
            user.ChangePicture(command.Picture);
            repo.Save(user, Guid.NewGuid());
        }
    }
}
