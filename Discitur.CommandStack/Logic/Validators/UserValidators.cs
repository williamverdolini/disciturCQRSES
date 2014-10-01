using CommonDomain.Persistence;
using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Model;
using Discitur.Infrastructure;
using Discitur.QueryStack;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.CommandStack.Logic.Validators
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        private readonly IDatabase database;

        public RegisterUserCommandValidator(IDatabase db)
        {
            Contract.Requires<ArgumentNullException>(db != null, "db");
            database = db;

            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.UserName).NotEmpty();
            RuleFor(command => command.UserName).Must(BeUniqueUserName).WithMessage("UserName is already used. Please choose another.");
            RuleFor(command => command.Email).EmailAddress();
            RuleFor(command => command.Email).Must(BeUniqueEmail).WithMessage("Email is already used. Please choose another."); ;
            RuleFor(command => command.Name).NotEmpty();
            RuleFor(command => command.Surname).NotEmpty();
        }

        private bool BeUniqueUserName(string userName)
        {
            return !database.Users.Any(u => u.UserName.Equals(userName));
        }

        private bool BeUniqueEmail(string email)
        {
            return !database.Users.Any(u => u.Email.Equals(email));
        }
    }

    public class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
    {
        private readonly IRepository repo;

        public ActivateUserCommandValidator(IRepository repository)
        {
            Contract.Requires<ArgumentNullException>(repository != null, "repository");
            repo = repository;

            RuleFor(command => command.UserName).NotEmpty();
            RuleFor(command => command.Key).NotEmpty();
            RuleFor(command => command.UserName).Must(BeWithUnactivatedKey).WithMessage("Wrong activation key for this user");
            RuleFor(command => command.Key).Must(BeNotActivatedUser).WithMessage("The user was already activated");
        }

        private bool BeWithUnactivatedKey(ActivateUserCommand command, string userName)
        {
            User _user = repo.GetById<User>(new Guid(command.Key));
            return _user.UserName.Equals(userName);
        }

        private bool BeNotActivatedUser(string key)
        {
            User _user = repo.GetById<User>(new Guid(key));
            return !repo.GetById<User>(new Guid(key)).IsActivated;
        }

    }

    public class ChangeUserEmailCommandValidator : AbstractValidator<ChangeUserEmailCommand>
    {
        private readonly IRepository repo;
        private readonly IDatabase database;

        public ChangeUserEmailCommandValidator(IRepository repository, IDatabase db)
        {
            Contract.Requires<ArgumentNullException>(repository != null, "repository");
            Contract.Requires<ArgumentNullException>(db != null, "db");
            repo = repository;
            database = db;

            RuleFor(command => command.UserName).NotEmpty();
            RuleFor(command => command.UserName).Must(BeCorrectUserName).WithMessage("UserName is not correct.");
            RuleFor(command => command.Email).NotEmpty();
            RuleFor(command => command.Email).Must(BeUniqueEmail).WithMessage("Email is already used. Please choose another."); ;
        }

        private bool BeCorrectUserName(ChangeUserEmailCommand command, string userName)
        {
            User user = repo.GetById<User>(command.Id);
            return user.UserName.Equals(userName);
        }

        private bool BeUniqueEmail(string email)
        {
            return !database.Users.Any(u => u.Email.Equals(email));
        }
    }

}
