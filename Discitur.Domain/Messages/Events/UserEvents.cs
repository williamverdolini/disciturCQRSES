using Discitur.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.Domain.Messages.Events
{
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

    public class ActivatedUserEvent : Event
    {
        public string UserName { get; private set; }

        public ActivatedUserEvent(Guid id, string userName)
        {
            UserName = userName;
            Id = id;
        }
    }

    public class ChangedUserEmailEvent : Event
    {
        public string Email { get; private set; }

        public ChangedUserEmailEvent(Guid id, string email)
        {
            Id = id;
            Email = email;
        }
    }

    public class ChangedUserPictureEvent : Event
    {
        public byte[] Picture { get; private set; }

        public ChangedUserPictureEvent(Guid id, byte[] picture)
        {
            Id = id;
            Picture = picture;
        }
    }



}
