using Discitur.Infrastructure;
using Discitur.Legacy.Migration.Infrastructure;
using Discitur.QueryStack;
using Discitur.QueryStack.Model;
using System;
using System.Linq;

namespace Discitur.Legacy.Migration.Model
{
    public class QueryIdsMigration : IMigrationStep, IQueryIdsMigration
    {
        private readonly IDatabase _db;

        public QueryIdsMigration(IDatabase database)
        {
            Contract.Requires<ArgumentNullException>(database != null, "database");
            _db = database;
        }

        public void Execute()
        {
            MapLessonFeedbacksId();
            MapLessonCommentsId();
            MapLessonRatingsId();
        }

        private void MapLessonFeedbacksId()
        {
            var feedbacks = from feedback in _db.LessonFeedbacks
                            where _db.IdMaps.GetAggregateId<LessonFeedback>(feedback.LessonFeedbackId).Equals(Guid.Empty)
                            select feedback;

            foreach (var lf in feedbacks)
            {
                // Get fresh new ID
                Guid entityId = Guid.NewGuid();
                while (!_db.IdMaps.GetModelId<LessonFeedback>(entityId).Equals(0))
                    entityId = Guid.NewGuid();
                // Map Ids
                _db.IdMaps.Map<LessonFeedback>(lf.LessonFeedbackId, entityId);
            }
        }

        private void MapLessonCommentsId()
        {
            var comments = from comment in _db.LessonComments
                           where _db.IdMaps.GetAggregateId<LessonComment>(comment.Id).Equals(Guid.Empty)
                           select comment;

            foreach (var c in comments)
            {
                // Get fresh new ID
                Guid entityId = Guid.NewGuid();
                while (!_db.IdMaps.GetModelId<LessonComment>(entityId).Equals(0))
                    entityId = Guid.NewGuid();
                // Map Ids
                _db.IdMaps.Map<LessonComment>(c.Id, entityId);
            }
        }

        private void MapLessonRatingsId()
        {
            var ratings = from rating in _db.LessonRatings
                          where _db.IdMaps.GetAggregateId<LessonRating>(rating.Id).Equals(Guid.Empty)
                          select rating;

            foreach (var c in ratings)
            {
                // Get fresh new ID
                Guid entityId = Guid.NewGuid();
                while (!_db.IdMaps.GetModelId<LessonRating>(entityId).Equals(0))
                    entityId = Guid.NewGuid();
                // Map Ids
                _db.IdMaps.Map<LessonRating>(c.Id, entityId);
            }
        }


    }
}
