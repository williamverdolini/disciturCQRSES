using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Messages.Events;
using Discitur.Infrastructure;
using Discitur.Infrastructure.Events;
using Discitur.QueryStack.Logic.Services;
using Discitur.QueryStack.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Discitur.QueryStack.Logic.EventHandlers
{
    public class LessonEventHandlers :
        IEventHandler<SavedNewDraftLessonEvent>,
        IEventHandler<SavedDraftLessonEvent>,
        IEventHandler<PublishedLessonEvent>,
        IEventHandler<UnPublishedLessonEvent>,
        IEventHandler<AddedNewCommentEvent>,
        IEventHandler<EditedCommentEvent>,
        IEventHandler<DeletedCommentEvent>,
        IEventHandler<AddedNewRatingEvent>,
        IEventHandler<EditedRatingEvent>,
        IEventHandler<DeletedRatingEvent>
    {
        private readonly IIdentityMapper _identityMapper;

        public LessonEventHandlers(IIdentityMapper identityMapper)
        {
            //Guard clauses
            Contract.Requires<ArgumentNullException>(identityMapper != null, "identityMapper");
            _identityMapper = identityMapper;
        }

        private void UpdateLessonArchFields(Lesson lesson, string lastModifUser, DateTime? lastModifDate, int version)
        {
            lesson.LastModifUser = lastModifUser;
            lesson.LastModifDate = lastModifDate;
            lesson.Vers = version + 1;
        }

        public void Handle(SavedNewDraftLessonEvent @event)
        {
            using (var db = new DisciturContext())
            {
                int userId = _identityMapper.GetModelId<User>(@event.AuthorId);
                User _user = db.Users.Find(userId);

                Lesson lesson = new Lesson();
                lesson.Title = @event.Title;
                lesson.Discipline = @event.Discipline;
                lesson.School = @event.School;
                lesson.Classroom = @event.Classroom;
                lesson.Author = _user;
                lesson.UserId = _user.UserId;
                lesson.Content = @event.Content;
                lesson.Conclusion = @event.Conclusion;
                lesson.PublishDate = @event.CreationDate;
                lesson.CreationDate = @event.CreationDate;
                UpdateLessonArchFields(lesson, _user.UserName, @event.CreationDate, @event.Version);
                //lesson.LastModifDate = @event.CreationDate;
                //lesson.LastModifUser = _user.UserName;
                //lesson.Vers = 1;
                lesson.RecordState = Constants.RECORD_STATE_ACTIVE;

                // Create FeedBacks Collection
                if (@event.FeedBacks.ContainsKey(EntityStatus.A))
                    @event.FeedBacks[EntityStatus.A].ToList()
                        .ForEach(feedback => {
                            LessonFeedback fb = new LessonFeedback()
                            {
                                Feedback = feedback.Feedback,
                                Nature = feedback.Nature,
                                LessonFeedbackGuid = feedback.Id
                            };
                            lesson.FeedBacks.Add(fb);
                        });
                // Create Tags Collection
                if (@event.Tags.ContainsKey(EntityStatus.A))
                    @event.Tags[EntityStatus.A].ToList()
                        .ForEach(tag =>
                        {
                            LessonTag t = new LessonTag()
                            {
                                LessonTagName = tag.LessonTagName
                            };
                            lesson.Tags.Add(t);
                        });

                db.Lessons.Add(lesson);
                db.SaveChanges();
                //Ids mapping
                _identityMapper.Map<Lesson>(lesson.LessonId, @event.Id);
                if (lesson.FeedBacks.Count > 0)
                    lesson.FeedBacks.ToList()
                        .ForEach(feedback => _identityMapper.Map<LessonFeedback>(feedback.LessonFeedbackId, feedback.LessonFeedbackGuid));
            }
        }

        public void Handle(SavedDraftLessonEvent @event)
        {
            using (var db = new DisciturContext())
            {
                int lessonId = _identityMapper.GetModelId<Lesson>(@event.Id);
                Lesson lesson = db.Lessons.Where(l => l.LessonId.Equals(lessonId) &&
                                            //l.Vers.Equals(@event.Version) &&
                                            l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
                                            .First();

                if (lesson.Title != @event.Title)
                    lesson.Title = @event.Title;
                if (lesson.Discipline != @event.Discipline)
                    lesson.Discipline = @event.Discipline;

                lesson.School = @event.School;
                lesson.Classroom = @event.Classroom;
                lesson.Discipline = @event.Discipline;
                lesson.Content = @event.Content;
                lesson.Conclusion = @event.Conclusion;
                lesson.PublishDate = @event.ModificationDate;

                // Update FeedBacks Collection
                if (@event.FeedBacks.ContainsKey(EntityStatus.A))
                    @event.FeedBacks[EntityStatus.A].ToList()
                        .ForEach(feedback => {
                            LessonFeedback fb = new LessonFeedback()
                            {
                                Feedback = feedback.Feedback,
                                Nature = feedback.Nature,
                                LessonFeedbackGuid = feedback.Id
                            };
                            lesson.FeedBacks.Add(fb);                    
                        });
                if (@event.FeedBacks.ContainsKey(EntityStatus.M))
                    @event.FeedBacks[EntityStatus.M].ToList()
                        .ForEach(feedback =>
                        {
                            var item = lesson.FeedBacks.Single(f => f.LessonFeedbackId.Equals(_identityMapper.GetModelId<LessonFeedback>(feedback.Id)));
                            item.Feedback = feedback.Feedback;
                        });
                if (@event.FeedBacks.ContainsKey(EntityStatus.C))
                    @event.FeedBacks[EntityStatus.C].ToList()
                        .ForEach(feedback =>
                        {
                            var item = lesson.FeedBacks.Single(f => f.LessonFeedbackId.Equals(_identityMapper.GetModelId<LessonFeedback>(feedback.Id)));
                            lesson.FeedBacks.Remove(item);
                            db.LessonFeedbacks.Remove(item);
                        });
                // Update Tags Collection
                if (@event.Tags.ContainsKey(EntityStatus.A))
                    @event.Tags[EntityStatus.A].ToList()
                        .ForEach(tag => {
                            LessonTag t = new LessonTag()
                            {
                                LessonTagName = tag.LessonTagName
                            };
                            lesson.Tags.Add(t);                    
                        });
                if (@event.Tags.ContainsKey(EntityStatus.C))
                    @event.Tags[EntityStatus.C].ToList()
                        .ForEach(tag =>
                        {
                            var item = lesson.Tags.Single(t => t.LessonTagName.Equals(tag.LessonTagName));
                            lesson.Tags.Remove(item);
                            db.LessonTags.Remove(item);
                        });
//                // update existing feedbacks
//                foreach (var item in @event.FeedBacks)
//                {
//                    LessonFeedback mfb;
//                    if (item.Id <= 0)
//                    {
//                        mfb = new LessonFeedback()
//                        {
////                            LessonId = lessonId,
//                            Feedback = item.Feedback,
////                            LessonFeedbackId = item.Id,
//                            Nature = item.Nature
//                        };
//                        lesson.FeedBacks.Add(mfb);
//                    }
//                    else
//                    {
//                        mfb = lesson.FeedBacks.FirstOrDefault(f => f.LessonFeedbackId.Equals(item.Id));
//                        if (mfb != null && mfb.Feedback != item.Feedback)
//                        {
//                            mfb.Feedback = item.Feedback;
//                            db.Entry(mfb).State = EntityState.Modified;
//                        }
//                    }
//                }
                //// remove deleted feedbacks
                //// deleted feedbacks exist in lesson.FeedBacks, but nt exit in @event.FeeBack
                //lesson.FeedBacks
                //    .Where(f => !@event.FeedBacks.Any(efb => efb.Id.Equals(f.LessonFeedbackId)))
                //    .ToList()
                //    .ForEach(item => db.LessonFeedbacks.Remove(item));

                //// remove deleted tags
                //// deleted tags exist in lesson.Tags, but nt exit in @event.Tags
                //lesson.Tags
                //    .Where(t => !@event.Tags.Any(et => et.LessonTagName.Equals(t.LessonTagName)))
                //    .ToList()
                //    .ForEach(item => db.LessonTags.Remove(item));
                //// add new tags
                //foreach (var item in @event.Tags)
                //{
                //    LessonTag t = lesson.Tags.FirstOrDefault(_t => _t.LessonId.Equals(lesson.LessonId) && _t.LessonTagName.Equals(item.LessonTagName));
                //    if (t == null)
                //    {
                //        LessonTag tag = new LessonTag();
                //        //tag.LessonId = lesson.LessonId;
                //        tag.LessonTagName = item.LessonTagName;
                //        //db.LessonTags.Add(tag);
                //        lesson.Tags.Add(tag);
                //    }
                //}

                //lesson.LastModifUser = lesson.LastModifUser;
                //lesson.LastModifDate = @event.ModificationDate;
                //lesson.Vers = @event.Version;
                UpdateLessonArchFields(lesson, lesson.LastModifUser, @event.ModificationDate, @event.Version);

                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();

                if (lesson.FeedBacks.Count > 0)
                    lesson.FeedBacks.ToList()
                        .ForEach(feedback => {
                            if (@event.FeedBacks.ContainsKey(EntityStatus.A) && @event.FeedBacks[EntityStatus.A].Any(f => f.Id.Equals(feedback.LessonFeedbackGuid)))
                                _identityMapper.Map<LessonFeedback>(feedback.LessonFeedbackId, feedback.LessonFeedbackGuid);                    
                        });

            }
        }

        public void Handle(PublishedLessonEvent @event)
        {
            using (var db = new DisciturContext())
            {
                int lessonId = _identityMapper.GetModelId<Lesson>(@event.Id);
                Lesson lesson = db.Lessons.Where(l => l.LessonId.Equals(lessonId) &&
                                            l.Vers.Equals(@event.Version) &&
                                            l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
                                            .First();
                lesson.PublishDate = @event.PublishDate;
                lesson.Published = Constants.LESSON_PUBLISHED;
                UpdateLessonArchFields(lesson, lesson.LastModifUser, @event.PublishDate, @event.Version);

                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void Handle(UnPublishedLessonEvent @event)
        {
            using (var db = new DisciturContext())
            {
                int lessonId = _identityMapper.GetModelId<Lesson>(@event.Id);
                Lesson lesson = db.Lessons.Where(l => l.LessonId.Equals(lessonId) &&
                                            l.Vers.Equals(@event.Version) &&
                                            l.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
                                            .First();
                lesson.Published = Constants.LESSON_NOT_PUBLISHED;
                UpdateLessonArchFields(lesson, lesson.LastModifUser, @event.UnPublishDate, @event.Version);

                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void Handle(AddedNewCommentEvent @event)
        {
            using (var db = new DisciturContext())
            {
                // get read-model Ids (ID-maps)
                int lessonId = _identityMapper.GetModelId<Lesson>(@event.Id);
                int userId = _identityMapper.GetModelId<User>(@event.AuthorId);
                int? parentId = @event.ParentId.Equals(Guid.Empty) ? null : (int?)_identityMapper.GetModelId<LessonComment>(@event.ParentId);
                // get involved read-model entities 
                User author = db.Users.Find(userId);
                Lesson lesson = db.Lessons.Find(lessonId);

                // Create new Read-Model Lesson's Comment
                LessonComment comment = new LessonComment();
                comment.Content = @event.Content;
                comment.CreationDate = @event.Date;
                comment.Date = @event.Date;
                comment.LastModifDate = @event.Date;
                comment.LessonId = lessonId;
                comment.Level = @event.Level;
                comment.ParentId = parentId;
                comment.Vers = 1;
                comment.RecordState = Constants.RECORD_STATE_ACTIVE;
                comment.UserId = userId;
                comment.Author = author;
                comment.LastModifUser = author.UserName;
                db.LessonComments.Add(comment);

                // Update Lesson's version
                // TODO: is it correct to update readModel "devops fields" of lesson in this case?
                UpdateLessonArchFields(lesson, author.UserName, @event.Date, @event.Version);
                // Persist changes
                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
                // Map new IDs
                // NOTE: Comment is NOT and AR, but it's mapped with it's own Id-map for compatibility with existant read-model
                _identityMapper.Map<LessonComment>(comment.Id, @event.CommentId);
            }
        }

        public void Handle(EditedCommentEvent @event)
        {
            using (var db = new DisciturContext())
            {
                // get read-model Ids (ID-maps)
                int lessonId = _identityMapper.GetModelId<Lesson>(@event.Id);
                int commentId = _identityMapper.GetModelId<LessonComment>(@event.CommentId);
                Lesson lesson = db.Lessons.Find(lessonId);
                LessonComment comment = db.LessonComments.Find(commentId);

                comment.Content = @event.Content;
                comment.Date = @event.Date;
                comment.LastModifDate = @event.Date;
                comment.Vers++;
                // Persist changes
                db.Entry(comment).State = EntityState.Modified;

                // TODO: is it correct to update readModel "devops fields" of lesson in this case?
                UpdateLessonArchFields(lesson, comment.LastModifUser, @event.Date, @event.Version);
                // Persist changes
                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void Handle(DeletedCommentEvent @event)
        {
            using (var db = new DisciturContext())
            {
                // get read-model Ids (ID-maps)
                int lessonId = _identityMapper.GetModelId<Lesson>(@event.Id);
                int commentId = _identityMapper.GetModelId<LessonComment>(@event.CommentId);
                Lesson lesson = db.Lessons.Find(lessonId);
                LessonComment comment = db.LessonComments.Find(commentId);

                comment.LastModifDate = @event.Date;
                comment.RecordState = Constants.RECORD_STATE_DELETED;
                // Persist changes
                db.Entry(comment).State = EntityState.Modified;

                // TODO: is it correct to update readModel "devops fields" of lesson in this case?
                UpdateLessonArchFields(lesson, comment.LastModifUser, @event.Date, @event.Version);
                // Persist changes
                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void Handle(AddedNewRatingEvent @event)
        {
            using (var db = new DisciturContext())
            {
                // get read-model Ids (ID-maps)
                int lessonId = _identityMapper.GetModelId<Lesson>(@event.Id);
                int userId = _identityMapper.GetModelId<User>(@event.UserId);
                // get involved read-model entities 
                User author = db.Users.Find(userId);
                Lesson lesson = db.Lessons.Find(lessonId);

                // Create new Read-Model Lesson's Comment
                LessonRating rating = new LessonRating();
                rating.LessonId = lessonId;
                rating.UserId = userId;
                rating.Author = author;
                rating.Rating = @event.Rating;
                rating.Content = @event.Content ?? string.Empty;
                rating.CreationDate = @event.Date;
                rating.LastModifDate = @event.Date;
                rating.LastModifUser = author.UserName;
                rating.Vers = 1;
                rating.RecordState = Constants.RECORD_STATE_ACTIVE;

                // Add new Lesson's Rating
                db.LessonRatings.Add(rating);
                //// Update Lesson Rating with Average of Ratings on the same Lesson
                //IQueryable<int> prevRatings = db.LessonRatings
                //                            .Where(r => r.LessonId.Equals(rating.LessonId) &&
                //                                r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
                //                            .Select(r => r.Rating);
                //List<int> ratingsList = prevRatings.ToList();
                //ratingsList.Add(rating.Rating);

                //lesson.Rate = Math.Max((int)Math.Round(ratingsList.Average()), 1);
                lesson.Rate = CalculateAverageRating(rating);

                // Update Lesson's version
                // TODO: is it correct to update readModel "devops fields" of lesson in this case?
                UpdateLessonArchFields(lesson, author.UserName, @event.Date, @event.Version);
                // Persist changes
                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
                // Map new IDs
                // NOTE: Comment is NOT and AR, but it's mapped with it's own Id-map for compatibility with existant read-model
                _identityMapper.Map<LessonRating>(rating.Id, @event.RatingId);
            }
        }

        public void Handle(EditedRatingEvent @event)
        {
            using (var db = new DisciturContext())
            {
                // get read-model Ids (ID-maps)
                int lessonId = _identityMapper.GetModelId<Lesson>(@event.Id);
                int ratingtId = _identityMapper.GetModelId<LessonRating>(@event.RatingId);
                Lesson lesson = db.Lessons.Find(lessonId);
                LessonRating rating = db.LessonRatings.Find(ratingtId);

                rating.Rating = @event.Rating;
                rating.Content = @event.Content;
                rating.LastModifDate = @event.Date;
                rating.Vers++;
                // Persist changes
                db.Entry(rating).State = EntityState.Modified;

                lesson.Rate = CalculateAverageRating(rating);
                // TODO: is it correct to update readModel "devops fields" of lesson in this case?
                UpdateLessonArchFields(lesson, rating.LastModifUser, @event.Date, @event.Version);
                // Persist changes
                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public void Handle(DeletedRatingEvent @event)
        {
            using (var db = new DisciturContext())
            {
                // get read-model Ids (ID-maps)
                int lessonId = _identityMapper.GetModelId<Lesson>(@event.Id);
                int ratingtId = _identityMapper.GetModelId<LessonRating>(@event.RatingId);
                Lesson lesson = db.Lessons.Find(lessonId);
                LessonRating rating = db.LessonRatings.Find(ratingtId);

                rating.LastModifDate = @event.Date;
                rating.RecordState = Constants.RECORD_STATE_DELETED;
                rating.Vers++;
                // Persist changes
                db.Entry(rating).State = EntityState.Modified;

                lesson.Rate = CalculateAverageRating(rating);
                // TODO: is it correct to update readModel "devops fields" of lesson in this case?
                UpdateLessonArchFields(lesson, rating.LastModifUser, @event.Date, @event.Version);
                // Persist changes
                db.Entry(lesson).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        private int CalculateAverageRating(LessonRating rating)
        {
            using (var db = new DisciturContext())
            {
                // Calculate Lesson Rating with Average of Ratings on the same Lesson
                IQueryable<int> prevRatings = db.LessonRatings
                                            .Where(r => r.LessonId.Equals(rating.LessonId) &&
                                                !r.Id.Equals(rating.Id) &&
                                                r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
                                            .Select(r => r.Rating);
                List<int> ratingsList = prevRatings.ToList();

                if (rating.RecordState.Equals(Constants.RECORD_STATE_ACTIVE))
                    ratingsList.Add(rating.Rating);

                return ratingsList.Count > 0 ? Math.Max((int)Math.Round(ratingsList.Average()), 1) : 0;
            }
        }
    }
}
