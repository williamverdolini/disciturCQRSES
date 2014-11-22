using Discitur.CommandStack.ViewModel;
using Discitur.Infrastructure.Api;
using System;

namespace Discitur.CommandStack.Worker
{
    public interface IUserCommandWorker : ICommandWorker
    {
        void RegisterUser(RegisterUserViewModel model);
        void ActivateUser(ActivateUserViewModel model);
        void ChangeUserEmail(ChangeUserEmailViewModel model);
        void ChangeUserPicture(ChangeUserPictureViewModel model);
        void LogInUser(int _userId, DateTime date);
    }
}
