using Discitur.Domain.Model;
using Discitur.Infrastructure.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discitur.Domain.Messages.Commands
{
    public enum EntityStatus
    {
        I,
        A,
        M,
        C
    }

    public class SaveNewDraftLessonCommand : Command
    {
        public string Title { get; private set; }
        public string Discipline { get; private set; }
        public string School { get; private set; }
        public string Classroom { get; private set; }
        //public int Rate { get; set; }
        public Guid AuthorId { get; private set; }
        //public DateTime? PublishDate { get; set; }
        public string Content { get; private set; }
        public string Conclusion { get; private set; }
        //public int Published { get; set; }
        public DateTime? CreationDate { get; private set; }
        //public DateTime? LastModifDate { get; set; }
        //public string LastModifUser { get; set; }
        //public int Vers { get; private set; }
        //public ICollection<LessonFeedback> FeedBacks { get; set; }
        //public ICollection<LessonTag> Tags { get; set; }
        //public ILookup<EntityStatus, LessonFeedback> FeedBacks { get; set; }
        //public ILookup<EntityStatus, LessonTag> Tags { get; set; }
        public IDictionary<EntityStatus, ICollection<LessonFeedback>> FeedBacks { get; set; }
        public IDictionary<EntityStatus, ICollection<LessonTag>> Tags { get; set; }

        public SaveNewDraftLessonCommand(Guid id, int version, string title, string discipline, string school, string classroom, Guid authorId, string content, string conclusion, IDictionary<EntityStatus, ICollection<LessonFeedback>> feedBacks, IDictionary<EntityStatus, ICollection<LessonTag>> tags)
        {
            Id = Guid.NewGuid();
            Version = version;
            //Id = id;
            Title = title;
            Discipline = discipline;
            School = school;
            Classroom = classroom;
            AuthorId = authorId;
            Content = content;
            Conclusion = conclusion;
            CreationDate = DateTime.Now;
            FeedBacks = feedBacks;
            Tags = tags;
        }
    }
       
    public class SaveDraftLessonCommand : Command
    {
        public string Title { get; private set; }
        public string Discipline { get; private set; }
        public string School { get; private set; }
        public string Classroom { get; private set; }
        public Guid AuthorId { get; private set; }
        public string Content { get; private set; }
        public string Conclusion { get; private set; }
        public DateTime? LastModifDate { get; private set; }
        //public ILookup<EntityStatus, LessonFeedback> FeedBacks { get; set; }
        //public ILookup<EntityStatus, LessonTag> Tags { get; set; }
        public IDictionary<EntityStatus, ICollection<LessonFeedback>> FeedBacks { get; set; }
        public IDictionary<EntityStatus, ICollection<LessonTag>> Tags { get; set; }

        public SaveDraftLessonCommand(Guid id, int version, string title, string discipline, string school, string classroom, Guid authorId, string content, string conclusion, DateTime? modificationDate, IDictionary<EntityStatus, ICollection<LessonFeedback>> feedBacks, IDictionary<EntityStatus, ICollection<LessonTag>> tags)
        {
            Id = id;
            Version = version;
            //Id = id;
            Title = title;
            Discipline = discipline;
            School = school;
            Classroom = classroom;
            AuthorId = authorId;
            Content = content;
            Conclusion = conclusion;
            LastModifDate = modificationDate;            
            FeedBacks = feedBacks;
            Tags = tags;
        }

    }

    public class PublishLessonCommand : Command
    {
        public DateTime? PublishDate { get; private set; }

        public PublishLessonCommand(Guid id, DateTime? publishDate, int vers)
        {
            Id = id;
            Version = vers;
            PublishDate = publishDate;
        }
    }

    public class UnPublishLessonCommand : Command
    {
        public DateTime? UnPublishDate { get; private set; }

        public UnPublishLessonCommand(Guid id, DateTime? unPublishDate, int vers)
        {
            Id = id;
            Version = vers;
            UnPublishDate = unPublishDate;
        }
    }
    
    public class AddNewCommentCommand : Command
    {
        //public int Id { get; set; }
        //public int LessonId { get; set; }
        public Guid CommentId { get; private set; }
        public Guid AuthorId { get; private set; }
        public string Content { get; private set; }
        public DateTime Date { get; private set; }
        public Guid ParentId { get; private set; }
        public int Level { get; private set; }

        public AddNewCommentCommand(Guid lessonId, Guid authorId, string content, DateTime date, Guid parentCommentId, int level)
        {
            Id = lessonId;
            CommentId = Guid.NewGuid();
            AuthorId = authorId;
            Content = content;
            Date = date;
            ParentId = parentCommentId;
            Level = level;
        }
    }

    public class EditCommentCommand : Command
    {
        public Guid CommentId { get; private set; }
        public string Content { get; private set; }
        public DateTime Date { get; private set; }

        public EditCommentCommand(Guid lessonId, Guid commentId, string content, DateTime date)
        {
            Id = lessonId;
            CommentId = commentId;
            Content = content;
            Date = date;
        }
    }

    public class DeleteCommentCommand : Command
    {
        public Guid CommentId { get; private set; }
        public DateTime Date { get; private set; }

        public DeleteCommentCommand(Guid lessonId, Guid commentId, DateTime date)
        {
            Id = lessonId;
            CommentId = commentId;
            Date = date;
        }
    }

    public class AddNewRatingCommand : Command
    {
        public Guid RatingId { get; private set; }
        public Guid UserId { get; private set; }
        public int Rating { get; private set; }
        public string Content { get; private set; }
        public DateTime Date { get; private set; }

        public AddNewRatingCommand(Guid lessonId, Guid userId, int rating, string content, DateTime date)
        {
            Id = lessonId;
            RatingId = Guid.NewGuid();
            UserId = userId;
            Rating = rating;
            Content = content;
            Date = date;
        }
    }

    public class EditRatingCommand : Command
    {
        public Guid RatingId { get; private set; }
        public int Rating { get; private set; }
        public string Content { get; private set; }
        public DateTime Date { get; private set; }

        public EditRatingCommand(Guid lessonId, Guid ratingId, int rating, string content, DateTime date)
        {
            Id = lessonId;
            RatingId = ratingId;
            Rating = rating;
            Content = content;
            Date = date;
        }
    }

    public class DeleteRatingCommand : Command
    {
        public Guid RatingId { get; private set; }
        public DateTime Date { get; set; }

        public DeleteRatingCommand(Guid lessonId, Guid ratingId, DateTime date)
        {
            Id = lessonId;
            RatingId = ratingId;
            Date = date;
        }
    }



}
