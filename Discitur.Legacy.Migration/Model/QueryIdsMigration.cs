using Discitur.Infrastructure;
using Discitur.Legacy.Migration.Infrastructure;
using Discitur.QueryStack;
using Discitur.QueryStack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Discitur.Legacy.Migration.Model
{
    public class QueryIdsMigration : IMigrationStep, IQueryIdsMigration
    {
        private readonly IDatabase _db;
        private IList<string> _logs;


        public QueryIdsMigration(IDatabase database)
        {
            Contract.Requires<ArgumentNullException>(database != null, "database");
            _db = database;
            _logs = new List<string>();
        }

        public IList<string> Execute()
        {
            MapLessonFeedbacksId();
            MapLessonCommentsId();
            MapLessonRatingsId();
            return _logs;
        }

        private void MapLessonFeedbacksId()
        {
            foreach (var feedback in _db.LessonFeedbacks)
            {
                if(_db.IdMaps.GetAggregateId<LessonFeedback>(feedback.LessonFeedbackId).Equals(Guid.Empty))
                {
                    // Get fresh new ID
                    Guid entityId = Guid.NewGuid();
                    while (!_db.IdMaps.GetModelId<LessonFeedback>(entityId).Equals(0))
                        entityId = Guid.NewGuid();
                    // Map Ids
                    _db.IdMaps.Map<LessonFeedback>(feedback.LessonFeedbackId, entityId);
                    _logs.Add(String.Format("{0} - Successfully Mapped LessonFeedbackId: {1}, Guid: {2}", DateTime.Now, feedback.LessonFeedbackId, entityId));
                }
            }
        }

        private void MapLessonCommentsId()
        {
            foreach (var comment in _db.LessonComments)
            {
                if(_db.IdMaps.GetAggregateId<LessonComment>(comment.Id).Equals(Guid.Empty))
                {
                    // Get fresh new ID
                    Guid entityId = Guid.NewGuid();
                    while (!_db.IdMaps.GetModelId<LessonComment>(entityId).Equals(0))
                        entityId = Guid.NewGuid();
                    // Map Ids
                    _db.IdMaps.Map<LessonComment>(comment.Id, entityId);
                    _logs.Add(String.Format("{0} - Successfully Mapped comment.Id: {1}, Guid: {2}", DateTime.Now, comment.Id, entityId));
                }
            }
        }

        private void MapLessonRatingsId()
        {
            foreach (var rating in _db.LessonRatings)
            {
                if(_db.IdMaps.GetAggregateId<LessonRating>(rating.Id).Equals(Guid.Empty))
                {
                    // Get fresh new ID
                    Guid entityId = Guid.NewGuid();
                    while (!_db.IdMaps.GetModelId<LessonRating>(entityId).Equals(0))
                        entityId = Guid.NewGuid();
                    // Map Ids
                    _db.IdMaps.Map<LessonRating>(rating.Id, entityId);
                    _logs.Add(String.Format("{0} - Successfully Mapped rating.Id: {1}, Guid: {2}", DateTime.Now, rating.Id, entityId));
                }
            }
        }


    }
}
