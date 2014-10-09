using CommonDomain;
using CommonDomain.Core;
using Discitur.Domain.Common;
using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Messages.Events;
using Discitur.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discitur.Domain.Model
{
    public class Lesson : AggregateBase, IMementoCreator
    {
        #region Public Properties
        public string Title { get; set; }
        public string Discipline { get; set; }
        public string School { get; set; }
        public string Classroom { get; set; }
        public int Rate { get; set; }
        //public IList<Rating> ratings
        public Guid AuthorId { get; set; }
        //virtual public User Author { get; set; }
        public DateTime? PublishDate { get; set; }
        public string Content { get; set; }
        public string Conclusion { get; set; }
        public int Published { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? LastModifDate { get; set; }
        public string LastModifUser { get; set; }
        public int RecordState { get; set; }
        public ICollection<LessonFeedback> FeedBacks { get; private set; }
        public ICollection<LessonTag> Tags { get; private set; }
        public ICollection<LessonComment> Comments { get; private set; }
        public ICollection<LessonRating> Ratings { get; private set; }
        #endregion

        #region Ctors
        //constructor with only id parameter for EventStore
        private Lesson(Guid id)
        {
            Id = id;
            Comments = new List<LessonComment>();
            Ratings = new List<LessonRating>();
        }
        
        //constructor with IMemento parameter for EventStore Snapshooting
        private Lesson(LessonMemento mementoItem)
        {
            Id = mementoItem.Id;
            Title = mementoItem.Title;
            Discipline = mementoItem.Discipline;
            School = mementoItem.School;
            Classroom = mementoItem.Classroom;
            AuthorId = mementoItem.AuthorId;
            Content = mementoItem.Content;
            Conclusion = mementoItem.Conclusion;
            CreationDate = mementoItem.CreationDate ?? DateTime.Now;
            LastModifDate = mementoItem.LastModifDate;
            Published = mementoItem.Published;
            RecordState = mementoItem.RecordState;
            FeedBacks = mementoItem.FeedBacks;
            Tags = mementoItem.Tags;
            Comments = new List<LessonComment>();
            Ratings = new List<LessonRating>();
        }

        public Lesson(Guid id, string title, string discipline, string school, string classroom, Guid authorId, string content, string conclusion, DateTime? creationDate, IDictionary<EntityStatus, ICollection<LessonFeedback>> feedBacks, IDictionary<EntityStatus, ICollection<LessonTag>> tags)
        {
            SaveNewDraftLesson(id, title, discipline, school, classroom, authorId, content, conclusion, creationDate, feedBacks, tags);
        }

        public IMemento CreateMemento()
        {
            return new LessonMemento(Id, Version, Title, Discipline, School, Classroom, Rate, AuthorId, PublishDate, Content, Conclusion, Published, CreationDate, LastModifDate, LastModifUser, RecordState, FeedBacks, Tags, Comments, Ratings);
        }
        #endregion

        #region Saved New Draft Lesson
        private void SaveNewDraftLesson(Guid id, string title, string discipline, string school, string classroom, Guid authorId, string content, string conclusion, DateTime? creationDate, IDictionary<EntityStatus, ICollection<LessonFeedback>> feedBacks, IDictionary<EntityStatus, ICollection<LessonTag>> tags)
        {
            RaiseEvent(new SavedNewDraftLessonEvent(id, title, discipline, school, classroom, authorId, content, conclusion, creationDate, feedBacks, tags));
        }

        void Apply(SavedNewDraftLessonEvent @event)
        {
            Id = @event.Id;
            Title = @event.Title;
            Discipline = @event.Discipline;
            School = @event.School;
            Classroom = @event.Classroom;
            AuthorId = @event.AuthorId;
            Content = @event.Content;
            Conclusion = @event.Conclusion;
            CreationDate = @event.CreationDate ?? DateTime.Now;
            FeedBacks = @event.FeedBacks.ContainsKey(EntityStatus.A) ? @event.FeedBacks[EntityStatus.A].ToList() : new List<LessonFeedback>();
            Tags = @event.Tags.ContainsKey(EntityStatus.A) ? @event.Tags[EntityStatus.A].ToList() : new List<LessonTag>();
            RecordState = Constants.RECORD_STATE_ACTIVE;
            Published = Constants.LESSON_NOT_PUBLISHED;
            Comments = new List<LessonComment>();
            Ratings = new List<LessonRating>();
        }
        #endregion

        #region Saved Draft Lesson
        public void SaveDraftLesson(string title, string discipline, string school, string classroom, Guid authorId, string content, string conclusion, DateTime? modificationDate, IDictionary<EntityStatus, ICollection<LessonFeedback>> feedBacks, IDictionary<EntityStatus, ICollection<LessonTag>> tags)
        {
            RaiseEvent(new SavedDraftLessonEvent(Id, Version, title, discipline, school, classroom, authorId, content, conclusion, modificationDate, feedBacks, tags));
        }

        void Apply(SavedDraftLessonEvent @event)
        {
            Title = @event.Title;
            Discipline = @event.Discipline;
            School = @event.School;
            Classroom = @event.Classroom;
            AuthorId = @event.AuthorId;
            Content = @event.Content;
            Conclusion = @event.Conclusion;
            LastModifDate = @event.ModificationDate;
            //FeedBacks.Concat(@event.FeedBacks[EntityState.New]);

            // Update FeedBacks Collection
            if (@event.FeedBacks.ContainsKey(EntityStatus.A))
                @event.FeedBacks[EntityStatus.A].ToList()
                    .ForEach(feedback => FeedBacks.Add(feedback));
            if (@event.FeedBacks.ContainsKey(EntityStatus.M))
                @event.FeedBacks[EntityStatus.M].ToList()
                .ForEach(feedback => { 
                    var item = FeedBacks.Single(f => f.Id.Equals(feedback.Id));
                    item.Feedback = feedback.Feedback;
                });
            if (@event.FeedBacks.ContainsKey(EntityStatus.C))
                @event.FeedBacks[EntityStatus.C].ToList()
                .ForEach(feedback => { 
                    var item = FeedBacks.Single(f => f.Id.Equals(feedback.Id));
                    FeedBacks.Remove(item);
                });
            // Update Tags Collection
            if (@event.Tags.ContainsKey(EntityStatus.A))
                @event.Tags[EntityStatus.A].ToList()
                .ForEach(tag => Tags.Add(tag));
            if (@event.Tags.ContainsKey(EntityStatus.C))
                @event.Tags[EntityStatus.C].ToList()
                .ForEach(tag =>
                {
                    var item = Tags.Single(f => f.LessonTagName.Equals(tag.LessonTagName));
                    Tags.Remove(item);
                });

            RecordState = Constants.RECORD_STATE_ACTIVE;
        }
        #endregion

        #region Published Lesson
        public void PublishLesson(DateTime? publishDate)
        {
            RaiseEvent(new PublishedLessonEvent(Id, Version, publishDate));
        }

        void Apply(PublishedLessonEvent @event)
        {
            PublishDate = @event.PublishDate ?? DateTime.Now;
            LastModifDate = PublishDate;
            Published = Constants.LESSON_PUBLISHED;
        }
        #endregion

        #region Un-Published Lesson
        public void UnPublishLesson(DateTime? unPublishDate)
        {
            RaiseEvent(new UnPublishedLessonEvent(Id, Version, unPublishDate));
        }

        void Apply(UnPublishedLessonEvent @event)
        {
            PublishDate = null;
            LastModifDate = @event.UnPublishDate;
            Published = Constants.LESSON_NOT_PUBLISHED;
        }
        #endregion
                
        #region Add new Lesson Comment
        public void AddNewComment(Guid commentId, Guid authorId, string content, DateTime date, Guid parentCommentId, int level)
        {
            RaiseEvent(new AddedNewCommentEvent(Id, Version, commentId, authorId, content, date, parentCommentId, level));
        }

        void Apply(AddedNewCommentEvent @event)
        {
            var comment = new LessonComment()
            {
                Id = @event.CommentId,
                AuthorId = @event.AuthorId,
                Content = @event.Content,
                CreationDate = @event.Date,
                LastModifDate = @event.Date,
                ParentId = @event.ParentId,
                Level = @event.Level,
                RecordState = Constants.RECORD_STATE_ACTIVE
            };
            Comments.Add(comment);
        }
        #endregion

        #region Edit Lesson Comment
        public void EditComment(Guid commentId, string content, DateTime date)
        {
            RaiseEvent(new EditedCommentEvent(Id, Version, commentId, content, date));
        }

        void Apply(EditedCommentEvent @event)
        {
            var comment = Comments.First(c => c.Id.Equals(@event.CommentId));
            if (comment != null)
            {
                comment.Content = @event.Content;
                comment.LastModifDate = @event.Date;
            }
            //TODO: raise domain exception, if comment is null

        }
        #endregion

        #region Delete Lesson Comment
        public void DeleteComment(Guid commentId, DateTime date)
        {
            RaiseEvent(new DeletedCommentEvent(Id, Version, commentId, date));
        }

        void Apply(DeletedCommentEvent @event)
        {
            var comment = Comments.First(c => c.Id.Equals(@event.CommentId));
            if (comment != null)
            {
                comment.LastModifDate = @event.Date;
                comment.RecordState = Constants.RECORD_STATE_DELETED;
            }
            //TODO: raise domain exception, if comment is null
        }
        #endregion

        #region Add new Lesson Rating
        public void AddNewRating(Guid ratingId, int rating, Guid userId, string content, DateTime date)
        {
            RaiseEvent(new AddedNewRatingEvent(Id, Version, ratingId, rating, userId, content, date));
        }

        void Apply(AddedNewRatingEvent @event)
        {
            var rating = new LessonRating()
            {
                Id = @event.RatingId,
                UserId = @event.UserId,
                Content = @event.Content,
                CreationDate = @event.Date,
                LastModifDate = @event.Date,
                RecordState = Constants.RECORD_STATE_ACTIVE
            };
            Ratings.Add(rating);
        }
        #endregion

        #region Edit Lesson Rating
        public void EditRating(Guid ratingtId, int rating, string content, DateTime date)
        {
            RaiseEvent(new EditedRatingEvent(Id, Version, ratingtId, rating, content, date));
        }

        void Apply(EditedRatingEvent @event)
        {
            var rating = Ratings.First(c => c.Id.Equals(@event.RatingId));
            if (rating != null)
            {
                rating.Content = @event.Content;
                rating.LastModifDate = @event.Date;
            }
            //TODO: raise domain exception, if comment is null

        }
        #endregion

        #region Delete Lesson Rating
        public void DeleteRating(Guid ratingId, DateTime date)
        {
            RaiseEvent(new DeletedRatingEvent(Id, Version, ratingId, date));
        }

        void Apply(DeletedRatingEvent @event)
        {
            var rating = Ratings.First(c => c.Id.Equals(@event.RatingId));
            if (rating != null)
            {
                rating.LastModifDate = @event.Date;
                rating.RecordState = Constants.RECORD_STATE_DELETED;
            }
            //TODO: raise domain exception, if comment is null
        }
        #endregion
    }

    //public struct Rating
    //{
    //    public int Value { get; private set; }
    //    public Rating(int value) : this()
    //    {
    //        Value = value;
    //    }
    //}

    public class LessonMemento : IMemento
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string Title { get; private set; }
        public string Discipline { get; private set; }
        public string School { get; private set; }
        public string Classroom { get; private set; }
        public int Rate { get; private set; }
        public Guid AuthorId { get; private set; }
        //virtual public User Author { get; private set; }
        public DateTime? PublishDate { get; private set; }
        public string Content { get; private set; }
        public string Conclusion { get; private set; }
        public int Published { get; private set; }
        public DateTime? CreationDate { get; private set; }
        public DateTime? LastModifDate { get; private set; }
        public string LastModifUser { get; private set; }
        public int RecordState { get; private set; }
        public ICollection<LessonFeedback> FeedBacks { get; private set; }
        public ICollection<LessonTag> Tags { get; private set; }
        public ICollection<LessonComment> Comments { get; private set; }
        public ICollection<LessonRating> Ratings { get; private set; }

        public LessonMemento(Guid id, int version, string title, string discipline, string school, string classroom, int rate, Guid authorId, DateTime? publishDate, string content, string conclusion, int published, DateTime? creationDate, DateTime? lastModifDate, string lastModifUser, int recordState, ICollection<LessonFeedback> feedBacks, ICollection<LessonTag> tags, ICollection<LessonComment> comments, ICollection<LessonRating> ratings)
        {
            Id = id;
            Version = version;
            Title = title;
            Discipline = discipline;
            School = school;
            Classroom = classroom;
            Rate = rate;
            AuthorId = authorId;
            PublishDate = publishDate;
            Content = content;
            Conclusion = conclusion;
            Published = published;
            CreationDate = creationDate;
            LastModifDate = lastModifDate;
            LastModifUser = lastModifUser;
            RecordState = recordState;
            FeedBacks = feedBacks;
            Tags = tags;
            Comments = comments;
            Ratings = ratings;
        }
    }
}
