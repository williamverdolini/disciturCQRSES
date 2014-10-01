using System.Linq;
using Discitur.QueryStack.Model;
using Discitur.QueryStack.Logic.Services;
using Discitur.Infrastructure;
using System;

namespace Discitur.QueryStack
{
    public class Database : IDatabase
    {
        private DisciturContext Context;
        private readonly IIdentityMapper _identityMapper;

        public Database(IIdentityMapper identityMapper)
        {
            Contract.Requires<ArgumentNullException>(identityMapper != null, "identityMapper");
            _identityMapper = identityMapper;

            Context = new DisciturContext();
            //Context.Configuration.AutoDetectChangesEnabled = false;
            // Lazy loading is turned off
            //Context.Configuration.LazyLoadingEnabled = false;
            Context.Database.Log = s => { System.Diagnostics.Debug.WriteLine(s); }; ; 
        }

        public IIdentityMapper IdMaps
        {
            get { return _identityMapper; }
        }

        public IQueryable<Lesson> Lessons
        {
            get { return Context.Lessons; }
        }

        public IQueryable<User> Users
        {
            get { return Context.Users; }
        }

        public IQueryable<UserActivation> UserActivations
        {
            get { return Context.UserActivations; }
        }

        public IQueryable<LessonFeedback> LessonFeedbacks
        {
            get { return Context.LessonFeedbacks; }
        }

        public IQueryable<LessonTag> LessonTags
        {
            get { return Context.LessonTags; }
        }

        public IQueryable<LessonComment> LessonComments
        {
            get { return Context.LessonComments; }
        }

        public IQueryable<LessonRating> LessonRatings
        {
            get { return Context.LessonRatings; }
        }
    }
}
