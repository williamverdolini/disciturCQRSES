using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Model;
using Discitur.Infrastructure.Events;
using Discitur.Infrastructure.Events.Versioning;
using System;
using System.Collections.Generic;

namespace Discitur.Domain.Messages.Events
{
    [VersionedEvent("SavedNewDraftLessonEvent", 0)]
    public class SavedNewDraftLessonEvent : Event
    {
        public string Title { get; private set; }
        public string Discipline { get; private set; }
        public string School { get; private set; }
        public string Classroom { get; private set; }
        public Guid AuthorId { get; private set; }
        public string Content { get; private set; }
        public string Conclusion { get; private set; }
        public DateTime? CreationDate { get; private set; }
        //public ICollection<LessonFeedback> FeedBacks { get; private set; }
        //public ICollection<LessonTag> Tags { get; private set; }
        //public ILookup<EntityStatus, LessonFeedback> FeedBacks { get; set; }
        //public ILookup<EntityStatus, LessonTag> Tags { get; set; }
        public IDictionary<EntityStatus, ICollection<LessonFeedback>> FeedBacks { get; set; }
        public IDictionary<EntityStatus, ICollection<LessonTag>> Tags { get; set; }

        public SavedNewDraftLessonEvent(Guid id, string title, string discipline, string school, string classroom, Guid authorId, string content, string conclusion, DateTime? creationDate, IDictionary<EntityStatus, ICollection<LessonFeedback>> feedBacks, IDictionary<EntityStatus, ICollection<LessonTag>> tags)
        {
            Id = id;
            Title = title;
            Discipline = discipline;
            School = school;
            Classroom = classroom;
            AuthorId = authorId;
            Content = content;
            Conclusion = conclusion;
            CreationDate = creationDate;
            FeedBacks = feedBacks;
            Tags = tags;
        }
    }

    [VersionedEvent("SavedDraftLessonEvent", 0)]
    public class SavedDraftLessonEvent : Event
    {
        public string Title { get; private set; }
        public string Discipline { get; private set; }
        public string School { get; private set; }
        public string Classroom { get; private set; }
        public Guid AuthorId { get; private set; }
        public string Content { get; private set; }
        public string Conclusion { get; private set; }
        public DateTime? ModificationDate { get; private set; }
        //public ICollection<LessonFeedback> FeedBacks { get; private set; }
        //public ICollection<LessonTag> Tags { get; private set; }
        //public ILookup<EntityStatus, LessonFeedback> FeedBacks { get; set; }
        //public ILookup<EntityStatus, LessonTag> Tags { get; set; }
        public IDictionary<EntityStatus, ICollection<LessonFeedback>> FeedBacks { get; set; }
        public IDictionary<EntityStatus, ICollection<LessonTag>> Tags { get; set; }


        public SavedDraftLessonEvent(Guid id, int version, string title, string discipline, string school, string classroom, Guid authorId, string content, string conclusion, DateTime? modificationDate, IDictionary<EntityStatus, ICollection<LessonFeedback>> feedBacks, IDictionary<EntityStatus, ICollection<LessonTag>> tags)
        {
            Id = id;
            Version = version;
            Title = title;
            Discipline = discipline;
            School = school;
            Classroom = classroom;
            AuthorId = authorId;
            Content = content;
            Conclusion = conclusion;
            ModificationDate = modificationDate;
            FeedBacks = feedBacks;
            Tags = tags;
        }
    }

    [VersionedEvent("PublishedLessonEvent", 0)]
    public class PublishedLessonEvent : Event
    {
        //public string Title { get; private set; }
        //public string Discipline { get; private set; }
        //public string School { get; private set; }
        //public string Classroom { get; private set; }
        //public Guid AuthorId { get; private set; }
        //public string Content { get; private set; }
        //public string Conclusion { get; private set; }
        public DateTime? PublishDate { get; private set; }
        //public ICollection<LessonFeedback> FeedBacks { get; private set; }
        //public ICollection<LessonTag> Tags { get; private set; }
        //public DateTime? LastModifDate { get; private set; }

        //public PublishedLessonEvent(Guid id, string title, string discipline, string school, string classroom, Guid authorId, string content, string conclusion, DateTime? publishDate, ICollection<LessonFeedback> feedBacks, ICollection<LessonTag> tags/*, DateTime? lastModifDate, string lastModifUser*/)
        //{
        //    Id = id;
        //    Title = title;
        //    Discipline = discipline;
        //    School = school;
        //    Classroom = classroom;
        //    AuthorId = authorId;
        //    Content = content;
        //    Conclusion = conclusion;
        //    PublishDate = publishDate;
        //    FeedBacks = feedBacks;
        //    Tags = tags;
        //}
        public PublishedLessonEvent(Guid id, int version, DateTime? publishDate)
        {
            Id = id;
            Version = version;
            PublishDate = publishDate;
        }
    }

    [VersionedEvent("UnPublishedLessonEvent", 0)]
    public class UnPublishedLessonEvent : Event
    {
        public DateTime? UnPublishDate { get; private set; }

        public UnPublishedLessonEvent(Guid id, int version, DateTime? unPublishDate)
        {
            Id = id;
            Version = version;
            UnPublishDate = unPublishDate;
        }
    }

    [VersionedEvent("AddedNewCommentEvent", 0)]
    public class AddedNewCommentEvent : Event
    {
        public Guid CommentId { get; private set; }
        public Guid AuthorId { get; private set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public Guid ParentId { get; set; }
        public int Level { get; set; }

        public AddedNewCommentEvent(Guid id, int version, Guid commentId, Guid authorId, string content, DateTime date, Guid parentCommentId, int level)
        {
            Id = id;
            Version = version;
            CommentId = commentId;
            AuthorId = authorId;
            Content = content;
            Date = date;
            ParentId = parentCommentId;
            Level = level;
        }
    }

    [VersionedEvent("EditedCommentEvent", 0)]
    public class EditedCommentEvent : Event
    {
        public Guid CommentId { get; private set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }

        public EditedCommentEvent(Guid id, int version, Guid commentId, string content, DateTime date)
        {
            Id = id;
            Version = version;
            CommentId = commentId;
            Content = content;
            Date = date;
        }
    }

    [VersionedEvent("DeletedCommentEvent", 0)]
    public class DeletedCommentEvent : Event
    {
        public Guid CommentId { get; private set; }
        public DateTime Date { get; set; }

        public DeletedCommentEvent(Guid id, int version, Guid commentId, DateTime date)
        {
            Id = id;
            Version = version;
            CommentId = commentId;
            Date = date;
        }
    }

    [VersionedEvent("AddedNewRatingEvent", 0)]
    public class AddedNewRatingEvent : Event
    {
        public Guid RatingId { get; private set; }
        public Guid UserId { get; private set; }
        public int Rating { get; private set; }
        public string Content { get; private set; }
        public DateTime Date { get; private set; }

        public AddedNewRatingEvent(Guid id, int version, Guid ratingId, int rating, Guid userId, string content, DateTime date)
        {
            Id = id;
            Version = version;
            RatingId = ratingId;
            Rating = rating;
            UserId = userId;
            Content = content;
            Date = date;
        }
    }

    [VersionedEvent("EditedRatingEvent", 0)]
    public class EditedRatingEvent : Event
    {
        public Guid RatingId { get; private set; }
        public int Rating { get; private set; }
        public string Content { get; private set; }
        public DateTime Date { get; private set; }

        public EditedRatingEvent(Guid id, int version, Guid ratingId, int rating, string content, DateTime date)
        {
            Id = id;
            Version = version;
            RatingId = ratingId;
            Rating = rating;
            Content = content;
            Date = date;
        }
    }

    [VersionedEvent("DeletedRatingEvent", 0)]
    public class DeletedRatingEvent : Event
    {
        public Guid RatingId { get; private set; }
        public DateTime Date { get; private set; }

        public DeletedRatingEvent(Guid id, int version, Guid ratingId, DateTime date)
        {
            Id = id;
            Version = version;
            RatingId = ratingId;
            Date = date;
        }
    }

    [VersionedEvent("LessonMementoPropagatedEvent", 0)]
    public class LessonMementoPropagatedEvent : Event
    {
        public LessonMemento Memento { get; private set; }

        public LessonMementoPropagatedEvent(LessonMemento memento)
        {
            Memento = memento;
        }

    }
}
