using Discitur.Domain.Messages.Events;
using Discitur.Infrastructure;
using Discitur.Legacy.Migration.Infrastructure;
using Discitur.QueryStack;
using Discitur.QueryStack.Model;
using NEventStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;

namespace Discitur.Legacy.Migration.Model
{
    public class LessonMigration : IMigrationStep, ILessonMigration
    {
        private readonly IDatabase _db;
        private readonly IStoreEvents _store;

        public LessonMigration(IDatabase database, IStoreEvents storeEvent)
        {
            Contract.Requires<ArgumentNullException>(database != null, "database");
            Contract.Requires<ArgumentNullException>(storeEvent != null, "storeEvent");
            _db = database;
            _store = storeEvent;
        }

        public void Execute()
        {
            foreach (var lesson in _db.Lessons.ToList())
            {
                if (_db.IdMaps.GetAggregateId<Lesson>(lesson.LessonId).Equals(Guid.Empty))
                {
                    // Get fresh new ID
                    Guid entityId = Guid.NewGuid();
                    while (!_db.IdMaps.GetModelId<Lesson>(entityId).Equals(0))
                        entityId = Guid.NewGuid();

                    // Save Ids mapping
                    _db.IdMaps.Map<Lesson>(lesson.LessonId, entityId);

                    // Create Memento from ReadModel
                    Guid userId = _db.IdMaps.GetAggregateId<User>(lesson.UserId);
                    var entity = new Discitur.Domain.Model.LessonMemento(
                        entityId,
                        1,
                        lesson.Title,
                        lesson.Discipline,
                        lesson.School,
                        lesson.Classroom,
                        lesson.Rate,
                        userId,
                        lesson.PublishDate,
                        lesson.Content,
                        lesson.Conclusion,
                        lesson.Published,
                        lesson.CreationDate,
                        lesson.LastModifDate,
                        lesson.LastModifUser,
                        lesson.RecordState,
                        GetLessonFeedbacks(lesson),
                        GetLessonTags(lesson),
                        GetLessonComments(lesson),
                        GetLessonRatings(lesson));

                    // Create a fake External event
                    using (var stream = _store.OpenStream(entityId, 0, int.MaxValue))
                    {
                        // Memento Propagation Event
                        var propagationEvent = new LessonMementoPropagatedEvent(entity);

                        stream.AddMigrationCommit(entity.GetType(), propagationEvent);
                        //stream.Add(new EventMessage { Body = entity });
                        //stream.CommitChanges(Guid.NewGuid());
                    }

                    // Save Snapshot from entity's Memento image
                    _store.Advanced.AddSnapshot(new Snapshot(entity.Id.ToString(), entity.Version, entity));

                    Trace.WriteLine(String.Format("Successfully migrated Lesson id: {0}, Guid: {1}, Title: {2}", lesson.LessonId, entityId.ToString(), lesson.Title), "Migration Process");
                }
            }
        }

        private ICollection<Domain.Model.LessonFeedback> GetLessonFeedbacks(Lesson lesson)
        {
            ICollection<Domain.Model.LessonFeedback> DomainFeedbacks = new List<Domain.Model.LessonFeedback>();
            foreach (var feedback in lesson.FeedBacks)
            {
                DomainFeedbacks.Add(new Domain.Model.LessonFeedback()
                {
                    Id = _db.IdMaps.GetAggregateId<LessonFeedback>(feedback.LessonFeedbackId),
                    Feedback = feedback.Feedback,
                    Nature = feedback.Nature
                }
                );
            }
            return DomainFeedbacks;
        }

        private ICollection<Domain.Model.LessonTag> GetLessonTags(Lesson lesson)
        {
            ICollection<Domain.Model.LessonTag> DomainTags = new List<Domain.Model.LessonTag>();
            foreach (var tag in lesson.Tags)
            {
                DomainTags.Add(new Domain.Model.LessonTag()
                {
                    LessonTagName = tag.LessonTagName
                }
                );
            }
            return DomainTags;
        }

        private ICollection<Domain.Model.LessonComment> GetLessonComments(Lesson lesson)
        {
            ICollection<Domain.Model.LessonComment> DomainComments = new List<Domain.Model.LessonComment>();
            var lessonComments = (from c in _db.LessonComments
                                 where c.LessonId.Equals(lesson.LessonId)
                                 select c).ToList();

            foreach (var comment in lessonComments)
            {
                DomainComments.Add(new Domain.Model.LessonComment()
                {
                    Id = _db.IdMaps.GetAggregateId<LessonComment>(comment.Id),
                    AuthorId = _db.IdMaps.GetAggregateId<User>(comment.UserId),
                    Content = comment.Content,
                    CreationDate = comment.CreationDate,
                    Date = comment.Date,
                    //LessonId = _db.IdMaps.GetAggregateId<User>(comment.UserId),
                    Level = comment.Level,
                    ParentId = comment.ParentId == null ? null : (Guid?)_db.IdMaps.GetAggregateId<LessonComment>(comment.ParentId.Value),
                    RecordState = comment.RecordState,
                    Vers = comment.Vers
                }
                );
            }
            return DomainComments;
        }

        private ICollection<Domain.Model.LessonRating> GetLessonRatings(Lesson lesson)
        {
            ICollection<Domain.Model.LessonRating> DomainRatings = new List<Domain.Model.LessonRating>();
            var lessonRatings = from c in _db.LessonRatings
                                where c.LessonId.Equals(lesson.LessonId)
                                select c;

            foreach (var rating in lessonRatings)
            {
                DomainRatings.Add(new Domain.Model.LessonRating()
                {
                    Id = _db.IdMaps.GetAggregateId<LessonRating>(rating.Id),
                    UserId = _db.IdMaps.GetAggregateId<User>(rating.UserId),
                    Content = rating.Content,
                    CreationDate = rating.CreationDate,
                    LastModifDate = rating.LastModifDate,
                    LastModifUser = rating.LastModifUser,
                    Rating = rating.Rating,
                    RecordState = rating.RecordState,
                    Vers = rating.Vers
                }
                );
            }
            return DomainRatings;
        }

    }
}
