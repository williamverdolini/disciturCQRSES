using System.Linq;
using Discitur.QueryStack.Model;
using Discitur.QueryStack.Logic.Services;

namespace Discitur.QueryStack
{
    public interface IDatabase
    {
        IQueryable<Lesson> Lessons { get; }

        IQueryable<User> Users { get; }

        IQueryable<UserActivation> UserActivations { get; }

        IQueryable<LessonFeedback> LessonFeedbacks { get; }

        IQueryable<LessonTag> LessonTags { get; }

        IQueryable<LessonComment> LessonComments { get; }

        IQueryable<LessonRating> LessonRatings { get; }

        IIdentityMapper IdMaps { get; }
    }
}
