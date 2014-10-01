using Discitur.CommandStack.ViewModel;
using Discitur.Infrastructure.Api;

namespace Discitur.CommandStack.Worker
{
    public interface ILessonCommandWorker : ICommandWorker
    {
        void SaveNewDraftLesson(SaveDraftLessonViewModel model);
        void SaveDraftLesson(SaveLessonAndPublishViewModel model);
        //void PublishLesson(SaveLessonAndPublishViewModel model);

        void AddNewComment(CommentViewModel model);
        void EditComment(CommentViewModel model);
        void DeleteComment(CommentViewModel model);
        void AddNewRating(RatingViewModel model);
        void EditRating(RatingViewModel model);
        void DeleteRating(RatingViewModel model);
    }
}
