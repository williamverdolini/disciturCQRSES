using CommonDomain.Persistence;
using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Model;
using Discitur.Infrastructure;
using Discitur.Infrastructure.Commands;
using NEventStore;
using System;

namespace Discitur.CommandStack.Logic.CommandHandlers
{
    public class LessonCommandHandlers :
        ICommandHandler<SaveNewDraftLessonCommand>,
        ICommandHandler<SaveDraftLessonCommand>,
        ICommandHandler<PublishLessonCommand>,
        ICommandHandler<UnPublishLessonCommand>,
        ICommandHandler<AddNewCommentCommand>,
        ICommandHandler<EditCommentCommand>,
        ICommandHandler<DeleteCommentCommand>,
        ICommandHandler<AddNewRatingCommand>,
        ICommandHandler<EditRatingCommand>,
        ICommandHandler<DeleteRatingCommand>
    {
        // Repository to get/save Aggregates/Entities from/to Domain Model
        private readonly IRepository repo;
        // Event Store to Raise (Extra-Domain) Events
        private readonly IStoreEvents store;

        public LessonCommandHandlers(IRepository repository, IStoreEvents eventStore) 
            //TODO: for Snapshooting ctor
            //: base(repository, eventStore)
        {
            //Guard clauses
            Contract.Requires<ArgumentNullException>(repository != null, "repository");
            Contract.Requires<ArgumentNullException>(eventStore != null, "eventStore");
            repo = repository;
            store = eventStore;
        }

        public void Handle(SaveNewDraftLessonCommand command)
        {
            Lesson lesson = new Lesson(command.Id, command.Title, command.Discipline, command.School, command.Classroom, command.AuthorId, command.Content, command.Conclusion, command.CreationDate, command.FeedBacks, command.Tags);
            repo.Save(lesson, Guid.NewGuid());
        }


        public void Handle(SaveDraftLessonCommand command)
        {
            Lesson lesson = repo.GetById<Lesson>(command.Id, command.Version);
            lesson.SaveDraftLesson(command.Title, command.Discipline, command.School, command.Classroom, command.AuthorId, command.Content, command.Conclusion, command.LastModifDate, command.FeedBacks, command.Tags);
            repo.Save(lesson, Guid.NewGuid());
        }

        public void Handle(PublishLessonCommand command)
        {
            Lesson lesson = repo.GetById<Lesson>(command.Id, command.Version);
            lesson.PublishLesson(command.PublishDate);
            repo.Save(lesson, Guid.NewGuid());
        }

        public void Handle(UnPublishLessonCommand command)
        {
            Lesson lesson = repo.GetById<Lesson>(command.Id, command.Version);
            lesson.UnPublishLesson(command.UnPublishDate);
            repo.Save(lesson, Guid.NewGuid());
        }

        public void Handle(AddNewCommentCommand command)
        {
            //Lesson lesson = repo.GetById<Lesson>(command.Id, command.Version);
            Lesson lesson = repo.GetById<Lesson>(command.Id);
            lesson.AddNewComment(command.CommentId, command.AuthorId, command.Content, command.Date, command.ParentId, command.Level);
            repo.Save(lesson, Guid.NewGuid());
        }

        public void Handle(EditCommentCommand command)
        {
            //Lesson lesson = repo.GetById<Lesson>(command.Id, command.Version);
            Lesson lesson = repo.GetById<Lesson>(command.Id);
            lesson.EditComment(command.CommentId, command.Content, command.Date);
            repo.Save(lesson, Guid.NewGuid());
        }

        public void Handle(DeleteCommentCommand command)
        {
            //TODO: the deleteComment needs optmistic lock -> add lesson version!!
            //Lesson lesson = repo.GetById<Lesson>(command.Id, command.Version);
            Lesson lesson = repo.GetById<Lesson>(command.Id);
            lesson.DeleteComment(command.CommentId, command.Date);
            repo.Save(lesson, Guid.NewGuid());
        }

        public void Handle(AddNewRatingCommand command)
        {
            //Lesson lesson = repo.GetById<Lesson>(command.Id, command.Version);
            Lesson lesson = repo.GetById<Lesson>(command.Id);
            lesson.AddNewRating(command.RatingId, command.Rating, command.UserId, command.Content, command.Date);
            repo.Save(lesson, Guid.NewGuid());
        }

        public void Handle(EditRatingCommand command)
        {
            //Lesson lesson = repo.GetById<Lesson>(command.Id, command.Version);
            Lesson lesson = repo.GetById<Lesson>(command.Id);
            lesson.EditRating(command.RatingId, command.Rating, command.Content, command.Date);
            repo.Save(lesson, Guid.NewGuid());
        }

        public void Handle(DeleteRatingCommand command)
        {
            //Lesson lesson = repo.GetById<Lesson>(command.Id, command.Version);
            Lesson lesson = repo.GetById<Lesson>(command.Id);
            lesson.DeleteRating(command.RatingId, command.Date);
            repo.Save(lesson, Guid.NewGuid());
        }
    }
}
