using Discitur.Domain.Model;
using Discitur.Infrastructure.Events;
using Discitur.Infrastructure.Events.Versioning;
using System;

namespace Discitur.Domain.Messages.Events
{
    [VersionedEvent("RegisteredUserEvent", 0)]
    public class RegisteredUserEvent : Event
    {
        public string Name { get; private set; }
        public string Surname { get; private set; }
        public string Email { get; private set; }
        public string UserName { get; private set; }

        public RegisteredUserEvent(Guid id, string name, string surName, string email, string userName)
        {
            Id = id;
            Name = name;
            Surname = surName;
            Email = email;
            UserName = userName;
        }
    }

    [VersionedEvent("ActivatedUserEvent", 0)]
    public class ActivatedUserEvent : Event
    {
        public string UserName { get; private set; }

        public ActivatedUserEvent(Guid id, string userName)
        {
            UserName = userName;
            Id = id;
        }
    }

    [VersionedEvent("ChangedUserEmailEvent", 0)]
    public class ChangedUserEmailEvent : Event
    {
        public string Email { get; private set; }

        public ChangedUserEmailEvent(Guid id, string email)
        {
            Id = id;
            Email = email;
        }
    }

    [VersionedEvent("ChangedUserPictureEvent", 0)]
    public class ChangedUserPictureEvent : Event
    {
        public byte[] Picture { get; private set; }

        public ChangedUserPictureEvent(Guid id, byte[] picture)
        {
            Id = id;
            Picture = picture;
        }
    }

    [VersionedEvent("LoggedInUserEvent", 0)]
    public class LoggedInUserEvent : Event
    {
        public DateTime Date { get; private set; }

        public LoggedInUserEvent(Guid id, DateTime date)
        {
            Id = id;
            Date = date;
        }
    }

    [VersionedEvent("UserMementoPropagatedEvent", 0)]
    public class UserMementoPropagatedEvent : Event
    {
        public UserMemento Memento { get; private set; }

        public UserMementoPropagatedEvent(UserMemento memento)
        {
            Memento = memento;
        }

    }

    [VersionedEvent("AchievedAffecionatedUserBadgeEvent", 0)]
    public class AchievedAffecionatedUserBadgeEvent : Event
    {
        public DateTime Date { get; private set; }

        public AchievedAffecionatedUserBadgeEvent(Guid id, DateTime date)
        {
            Id = id;
            Date = date;
        }
    }    
}
