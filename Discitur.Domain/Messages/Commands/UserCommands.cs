using Discitur.Infrastructure.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.Domain.Messages.Commands
{
    public class RegisterUserCommand : Command
    {
        public string Name { get; private set; }
        public string Surname { get; private set; }
        public string Email { get; private set; }
        public string UserName { get; private set; }

        public RegisterUserCommand(string name, string surName, string email, string userName)
        {
            Id = Guid.NewGuid();
            Name = name;
            Surname = surName;
            Email = email;
            UserName = userName;
        }
    }

    public class ActivateUserCommand : Command
    {
        public string UserName { get; set; }
        public string Key { get; set; }

        public ActivateUserCommand(string userName, string key)
        {
            UserName = userName;
            Key = key;
        }
    }

    public class ChangeUserEmailCommand : Command
    {
        public string Email { get; set; }
        public string UserName { get; set; }

        public ChangeUserEmailCommand(Guid id, string email, string userName)
        {
            Id = id;
            Email = email;
            UserName = userName;
        }
    }

    public class ChangeUserPictureCommand : Command
    {
        public byte[] Picture { get; private set; }

        public ChangeUserPictureCommand(Guid id, byte[] picture)
        {
            Id = id;
            Picture = picture;
        }
    }

}
