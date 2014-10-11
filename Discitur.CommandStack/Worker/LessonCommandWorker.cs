using CommonDomain.Persistence;
using Discitur.CommandStack.ViewModel;
using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Model;
using Discitur.Infrastructure;
using Discitur.QueryStack;
using System;

namespace Discitur.CommandStack.Worker
{
    public class LessonCommandWorker : ILessonCommandWorker
    {
        private readonly IBus bus;
        private readonly IDatabase database;
        private readonly IRepository repo;

        public LessonCommandWorker(IBus commandBus, IDatabase db, IRepository repository)
        {
            Contract.Requires<ArgumentNullException>(commandBus != null, "commandBus");
            Contract.Requires<ArgumentNullException>(db != null, "db");
            Contract.Requires<ArgumentNullException>(repository != null, "repository");

            bus = commandBus;
            database = db;
            repo = repository;
        }

        //private ICollection<LessonFeedback> ToLessonFeedbackCommands(ICollection<FeedbackViewModel> feedBackViewModel)
        //{
        //    ICollection<LessonFeedback> feedbacks = new HashSet<LessonFeedback>();
        //    foreach (var item in feedBackViewModel.Where(f => !f.Status.Equals(Discitur.Domain.Common.Constants.LESSON_FEEDBACK_REMOVED)))
        //    {
        //        LessonFeedback fb = new LessonFeedback()
        //        {
        //            Feedback = item.Feedback,
        //            Id = item.LessonFeedbackId ?? 0,
        //            Nature = item.Nature
        //        };
        //        _feedbacks.Add(fb);
        //    }

        //    return feedbacks;
        //}


        /// <summary>
        /// Create a Lookup collection to get New/Modified/Removed Lesson's feedbacks
        /// </summary>
        /// <param name="feedBacks"></param>
        /// <returns></returns>
        //private ILookup<EntityStatus, LessonFeedback> ToLessonFeedbacks(ICollection<FeedbackViewModel> feedBacks)
        //{
        //    ILookup<EntityStatus, LessonFeedback> _feedbacks = (Lookup<EntityStatus, LessonFeedback>)feedBacks
        //        .ToLookup(
        //            f => f.Status.ParseEnum<EntityStatus>(),
        //            f => new LessonFeedback()
        //                {
        //                    Id = f.LessonFeedbackId == null ? 
        //                            Guid.NewGuid() : database.IdMaps.GetAggregateId<LessonFeedback>(f.LessonFeedbackId.Value),
        //                    Feedback = f.Feedback,
        //                    Nature = f.Nature
        //                }
        //        );
        //    return _feedbacks;
        //}

        /// <summary>
        /// Create a Lookup collection to get New/Modified/Removed Lesson's tags
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        //private ILookup<EntityStatus, LessonTag> ToLessonTag(ICollection<TagViewModel> tags)
        //{
        //    ILookup<EntityStatus, LessonTag> _tags = (Lookup<EntityStatus, LessonTag>)tags
        //        .ToLookup(
        //            t => t.Status.ParseEnum<EntityStatus>(),
        //            t => new LessonTag()
        //            {
        //                LessonTagName = t.LessonTagName
        //            }
        //        );
        //    return _tags;
        //}


        public void SaveNewDraftLesson(SaveDraftLessonViewModel model)
        {
            Guid authorId = database.IdMaps.GetAggregateId<User>(model.Author.UserId);
            Guid lessonId = model.LessonId.Equals(0) ? Guid.Empty : database.IdMaps.GetAggregateId<Lesson>(model.LessonId);

            //Guid lessonId = Guid.NewGuid();
            SaveNewDraftLessonCommand command = new SaveNewDraftLessonCommand(lessonId, 1, model.Title, model.Discipline, model.School, model.Classroom, authorId, model.Content, model.Conclusion, model.Feedbacks.ToDictionary(database.IdMaps), model.Tags.ToDictionary());
            bus.Send<SaveNewDraftLessonCommand>(command);
            if (model.Published.Equals(Constants.LESSON_PUBLISHED))
                bus.Send<PublishLessonCommand>(new PublishLessonCommand(command.Id, DateTime.Now, command.Version));
            // Possibile only into in-process command-query thread
            // If async is necessary the Ids (both Repo and Database) should be generated into the command process
            model.LessonId = database.IdMaps.GetModelId<Lesson>(command.Id);
        }

        public void SaveDraftLesson(SaveLessonAndPublishViewModel model)
        {
            Guid authorId = database.IdMaps.GetAggregateId<User>(model.Author.UserId);
            Guid lessonId = database.IdMaps.GetAggregateId<Lesson>(model.LessonId);

            //Guid lessonId = Guid.NewGuid();
            SaveDraftLessonCommand command = new SaveDraftLessonCommand(lessonId, model.Vers, model.Title, model.Discipline, model.School, model.Classroom, authorId, model.Content, model.Conclusion, DateTime.Now, model.Feedbacks.ToDictionary(database.IdMaps), model.Tags.ToDictionary());
            bus.Send<SaveDraftLessonCommand>(command);

            Lesson lesson = repo.GetById<Lesson>(lessonId);
            if (lesson.Published.Equals(Constants.LESSON_NOT_PUBLISHED) && model.Published.Equals(Constants.LESSON_PUBLISHED))
                bus.Send<PublishLessonCommand>(new PublishLessonCommand(command.Id, DateTime.Now, model.Vers));
            else if(lesson.Published.Equals(Constants.LESSON_PUBLISHED) && model.Published.Equals(Constants.LESSON_NOT_PUBLISHED))
                bus.Send<UnPublishLessonCommand>(new UnPublishLessonCommand(command.Id, DateTime.Now, model.Vers));

            // Possibile only into in-process command-query thread
            // If async is necessary the Ids (both Repo and Database) should be generated into the command process
            model.LessonId = database.IdMaps.GetModelId<Lesson>(command.Id);
        }

        public void PublishLesson(SaveLessonAndPublishViewModel model)
        {
            Guid lessonId = database.IdMaps.GetAggregateId<Lesson>(model.LessonId);
            bus.Send<PublishLessonCommand>(new PublishLessonCommand(lessonId, DateTime.Now, model.Vers));
        }

        public void AddNewComment(CommentViewModel model)
        {
            Guid lessonId = database.IdMaps.GetAggregateId<Lesson>(model.LessonId);
            Guid authorId = database.IdMaps.GetAggregateId<User>(model.Author.UserId);
            Guid parentId = model.ParentId == null ? Guid.Empty : database.IdMaps.GetAggregateId<LessonComment>(model.ParentId.Value);
            DateTime date = model.Date == null ? DateTime.Now : model.Date.Value;

            var command = new AddNewCommentCommand(lessonId, authorId, model.Content, date, parentId, model.Level, model.Vers);
            bus.Send<AddNewCommentCommand>(command);
            model.Id = database.IdMaps.GetModelId<LessonComment>(command.CommentId);
        }

        public void EditComment(CommentViewModel model)
        {
            Guid lessonId = database.IdMaps.GetAggregateId<Lesson>(model.LessonId);
            Guid commentId = database.IdMaps.GetAggregateId<LessonComment>(model.Id.Value);
            DateTime date = model.Date == null ? DateTime.Now : model.Date.Value;

            bus.Send<EditCommentCommand>(new EditCommentCommand(lessonId, commentId, model.Content, date, model.Vers));
        }

        public void DeleteComment(CommentViewModel model)
        {
            Guid lessonId = database.IdMaps.GetAggregateId<Lesson>(model.LessonId);
            Guid commentId = database.IdMaps.GetAggregateId<LessonComment>(model.Id.Value);
            DateTime date = model.Date == null ? DateTime.Now : model.Date.Value;

            bus.Send<DeleteCommentCommand>(new DeleteCommentCommand(lessonId, commentId, date, model.Vers));
        }

        public void AddNewRating(RatingViewModel model)
        {
            Guid lessonId = database.IdMaps.GetAggregateId<Lesson>(model.LessonId);
            Guid authorId = database.IdMaps.GetAggregateId<User>(model.Author.UserId);
            DateTime date = model.Date == null ? DateTime.Now : model.Date.Value;

            var command = new AddNewRatingCommand(lessonId, authorId, model.Rating, model.Content, date, model.Vers);
            bus.Send<AddNewRatingCommand>(command);
            model.Id = database.IdMaps.GetModelId<LessonRating>(command.RatingId);
        }

        public void EditRating(RatingViewModel model)
        {
            Guid lessonId = database.IdMaps.GetAggregateId<Lesson>(model.LessonId);
            Guid ratingId = database.IdMaps.GetAggregateId<LessonRating>(model.Id.Value);
            DateTime date = model.Date == null ? DateTime.Now : model.Date.Value;

            bus.Send<EditRatingCommand>(new EditRatingCommand(lessonId, ratingId, model.Rating, model.Content, date, model.Vers));
        }

        public void DeleteRating(RatingViewModel model)
        {
            Guid lessonId = database.IdMaps.GetAggregateId<Lesson>(model.LessonId);
            Guid ratingId = database.IdMaps.GetAggregateId<LessonRating>(model.Id.Value);
            DateTime date = model.Date == null ? DateTime.Now : model.Date.Value;

            bus.Send<DeleteRatingCommand>(new DeleteRatingCommand(lessonId, ratingId, date, model.Vers));
        }
    }
}
