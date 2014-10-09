using CommonDomain;
using CommonDomain.Core;
using Discitur.Domain.Messages.Events;
using Discitur.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.Domain.Model
{
    public class User : AggregateBase, IMementoCreator
    {
        public string Name { get; private set; }
        public string Surname { get; private set; }
        public string Email { get; private set; }
        public string UserName { get; private set; }
        public byte[] Picture { get; private set; }
        public bool IsActivated { get; private set; }

        #region Constructors and Factories
        //constructor with only id parameter for EventStore
        private User(Guid UserId)
        {
            Id = UserId;
        }

        //constructor with IMemento parameter for EventStore Snapshooting
        private User(UserMemento mementoItem)
        {
            Id = mementoItem.Id;
            Version = mementoItem.Version;
            Name = mementoItem.Name;
            Surname = mementoItem.Surname;
            Email = mementoItem.Email;
            UserName = mementoItem.UserName;
            Picture = mementoItem.Picture;
            IsActivated = mementoItem.IsActivated;
        }

        public User(Guid id, string name, string surname, string email, string userName)
        {
            Id = id;
            Name = name;
            Surname = surname;
            Email = email;
            UserName = userName;
            IsActivated = false;
            // Raise Registration Event
            RegisterUser(id, name, surname, email, userName);
        }

        public IMemento CreateMemento()
        {
            return new UserMemento(Id, Version, Name, Surname, Email, UserName, Picture/*, Thumb*/, IsActivated);
        }
        #endregion

        #region Register new User
        private void RegisterUser(Guid id, string name, string surname, string email, string userName)
        {
            RaiseEvent(new RegisteredUserEvent(id, name, surname, email, userName));
        }

        void Apply(RegisteredUserEvent @event)
        {
            Id = @event.Id;
            Name = @event.Name;
            Surname = @event.Surname;
            Email = @event.Email;
            UserName = @event.UserName;
            IsActivated = false;
        }
        #endregion

        #region Activate new registered user
        public void Activate()
        {
            RaiseEvent(new ActivatedUserEvent(Id, UserName));
        }

        void Apply(ActivatedUserEvent @event)
        {
            IsActivated = true;
        }
        #endregion

        #region Change user's email
        public void ChangeEmail(string email)
        {
            RaiseEvent(new ChangedUserEmailEvent(Id, email));
        }

        void Apply(ChangedUserEmailEvent @event)
        {
            Email = @event.Email;
        }
        #endregion

        #region Change user's picture
        public void ChangePicture(byte[] picture)
        {
            RaiseEvent(new ChangedUserPictureEvent(Id, picture));
        }

        void Apply(ChangedUserPictureEvent @event)
        {
            Picture = @event.Picture;
        }
        #endregion
        
    }


    public class UserMemento : IMemento
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public byte[] Picture { get; set; }
        public bool IsActivated { get; private set; }

        public UserMemento(Guid id, int version, string name, string surname, string email, string userName, byte[] picture, /*string thumb,*/ bool isActivated)
        {
            Id = id;
            Version = version;
            Name = name;
            Surname = surname;
            Email = email;
            UserName = userName;
            Picture = picture;
            IsActivated = isActivated;
        }
    }
}
