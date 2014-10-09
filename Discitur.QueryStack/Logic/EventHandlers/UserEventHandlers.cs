using Discitur.Domain.Messages.Events;
using Discitur.Infrastructure;
using Discitur.Infrastructure.Events;
using Discitur.QueryStack.Logic.Services;
using Discitur.QueryStack.Model;
using ImageResizer;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace Discitur.QueryStack.Logic.EventHandlers
{
    public class UserEventHandlers : 
        IEventHandler<RegisteredUserEvent>,
        IEventHandler<ActivatedUserEvent>,
        IEventHandler<ChangedUserEmailEvent>,
        IEventHandler<ChangedUserPictureEvent>
    {
        private readonly IIdentityMapper _identityMapper;
        private readonly IImageConverter _imageConverter;

        public UserEventHandlers(IIdentityMapper identityMapper, IImageConverter imageConverter)
        {
            Contract.Requires<ArgumentNullException>(identityMapper != null, "identityMapper");
            Contract.Requires<ArgumentNullException>(imageConverter != null, "imageStringConverter");
            _identityMapper = identityMapper;
            _imageConverter = imageConverter;
        }

        public void Handle(RegisteredUserEvent @event)
        {
            using (var db = new DisciturContext())
            {
                // Add new User to Read-Model
                User discuser = new User
                {
                    Name = @event.Name,
                    Surname = @event.Surname,
                    Email = @event.Email,
                    UserName = @event.UserName,
                    Picture = Constants.USER_DEFAULT_PICTURE
                };                
                db.Users.Add(discuser);

                // Add new User-Activation Key to Read-Model
                UserActivation userActivation = new UserActivation
                {
                    UserName = @event.UserName,
                    Key = @event.Id.ToString()
                };
                db.UserActivations.Add(userActivation);
                db.SaveChanges();
                _identityMapper.Map<User>(discuser.UserId, @event.Id);
            }
        }

        public void Handle(ActivatedUserEvent @event)
        {
            using (var db = new DisciturContext())
            {
                UserActivation activation = db.UserActivations.Where(a => a.UserName.Equals(@event.UserName) && a.Key.Equals(@event.Id.ToString())).First();
                if (activation != null)
                {
                    db.UserActivations.Remove(activation);
                    db.SaveChanges();
                }
            }
        }

        public void Handle(ChangedUserEmailEvent @event)
        {
            using (var db = new DisciturContext())
            {
                // ATTENTION: I can use Discitur.QueryStack.Model.User, instead of Discitur.Domain.Model.User
                //            just because the class Name (User) is the same in both classes.
                //           This implementation of _identityMapper uses Class.Name to map (not FullName)
                int userId = _identityMapper.GetModelId<User>(@event.Id);
                User user = db.Users.Find(userId);
                if (user != null)
                {
                    user.Email = @event.Email;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public void Handle(ChangedUserPictureEvent @event)
        {
            using (var db = new DisciturContext())
            {
                int userId = _identityMapper.GetModelId<User>(@event.Id);
                User user = db.Users.Find(userId);

                //Stream stPictureSource = new MemoryStream(@event.Picture);
                //Stream stThumbSource = new MemoryStream(@event.Picture);
                //// Resize for Picture
                //MemoryStream stPictureDest = new MemoryStream();
                //var pictureSettings = new ResizeSettings
                //{
                //    MaxWidth = Constants.USER_PICTURE_MAXWIDTH,
                //    MaxHeight = Constants.USER_PICTURE_MAXHEIGHT,
                //    Format = Constants.USER_PICTURE_FORMAT,
                //    Mode = FitMode.Crop
                //};
                //ImageBuilder.Current.Build(stPictureSource, stPictureDest, pictureSettings);

                //// Resize for ThumbNail
                //MemoryStream stThumbDest = new MemoryStream();
                //var thumbSettings = new ResizeSettings
                //{
                //    MaxWidth = Constants.USER_THUMB_MAXWIDTH,
                //    MaxHeight = Constants.USER_THUMB_MAXHEIGHT,
                //    Format = Constants.USER_THUMB_FORMAT,
                //    Mode = FitMode.Crop
                //};
                //ImageBuilder.Current.Build(stThumbSource, stThumbDest, thumbSettings);

                //user.Picture = "data:image/gif;base64," + Convert.ToBase64String(stPictureDest.ToArray());
                //user.Thumb = "data:image/gif;base64," + Convert.ToBase64String(stThumbDest.ToArray());

                user.Picture = "data:image/gif;base64," + _imageConverter.ToPictureString(@event.Picture);
                user.Thumb = "data:image/gif;base64," + _imageConverter.ToThumbNailString(@event.Picture);
                
                db.SaveChanges();
            }
        }
    }
}
