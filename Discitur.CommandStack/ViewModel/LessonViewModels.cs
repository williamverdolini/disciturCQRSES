using Discitur.Domain.Common;
using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Model;
using Discitur.Infrastructure;
using Discitur.QueryStack;
using Discitur.QueryStack.Logic.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Discitur.CommandStack.ViewModel
{
    public class SaveDraftLessonViewModel
    {
        [Required]
        public int LessonId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Discipline { get; set; }
        [Required]
        public string School { get; set; }
        [Required]
        public string Classroom { get; set; }
        //[Required]
        //public int Rate { get; set; }
        //[Required]
        //public int UserId { get; set; }
        [Required]
        public UserViewModel Author { get; set; }
        //[Required]
        //public DateTime? PublishDate { get; set; }
        [Required]
        public string Content { get; set; }
        public string Conclusion { get; set; }
        public int Published { get; set; }
        //public int Vers { get; set; }
        //[Required]
        //public DateTime? CreationDate { get; set; }
        //[Required]
        //public DateTime? LastModifDate { get; set; }
        //[Required]
        //public string LastModifUser { get; set; }
        public ICollection<FeedbackViewModel> Feedbacks;
        public ICollection<TagViewModel> Tags;
    }

    public class SaveLessonAndPublishViewModel
    {
        [Required]
        public int LessonId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Discipline { get; set; }
        [Required]
        public string School { get; set; }
        [Required]
        public string Classroom { get; set; }
        public UserViewModel Author { get; set; }
        //[Required]
        public DateTime? PublishDate { get; set; }
        [Required]
        public string Content { get; set; }
        public string Conclusion { get; set; }
        public int Published { get; set; }
        public int Vers { get; set; }
        public string Status { get; set; }
        public ICollection<FeedbackViewModel> Feedbacks;
        public ICollection<TagViewModel> Tags;
    }

    public class UserViewModel
    {
        public int UserId { get; set; }
    }

    public class FeedbackViewModel
    {
        public int? LessonFeedbackId { get; set; }
        //public int LessonId { get; set; }
        //[ForeignKey("LessonId")]
        //public virtual Lesson Lesson { get; set; }
        public int Nature { get; set; }
        public string Feedback { get; set; }
        public string Status { get; set; }
    }

    public class TagViewModel
    {
        public string LessonTagName { get; set; }
        public string Status { get; set; }
        //public int LessonId { get; set; }
    }

    //TODO: Separate the class and create a Class in Infrastructure project (Utility)
    public static class MapViewsToModel
    {

        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        }

        public static IDictionary<EntityStatus, ICollection<LessonFeedback>> ToDictionary(this ICollection<FeedbackViewModel> feedBacks, IIdentityMapper maps)
        {
            IDictionary<EntityStatus, ICollection<LessonFeedback>> feedbackDict = new Dictionary<EntityStatus, ICollection<LessonFeedback>>();

            foreach (var item in feedBacks)
            {
                EntityStatus status = item.Status.ParseEnum<EntityStatus>();
                if(!status.Equals(EntityStatus.I))
                {
                    if (!feedbackDict.ContainsKey(status))
                    {
                        feedbackDict.Add(status, new List<LessonFeedback>());
                    }
                    feedbackDict[status].Add(
                            new LessonFeedback()
                            {
                                Id = item.LessonFeedbackId == null ?
                                    Guid.NewGuid() : maps.GetAggregateId<LessonFeedback>(item.LessonFeedbackId.Value),
                                Feedback = item.Feedback,
                                Nature = item.Nature
                            }
                        );
                }
            }
            return feedbackDict;
        }

        public static IDictionary<EntityStatus, ICollection<LessonTag>> ToDictionary(this ICollection<TagViewModel> tags)
        {
            IDictionary<EntityStatus, ICollection<LessonTag>> tagDictionary = new Dictionary<EntityStatus, ICollection<LessonTag>>();

            foreach (var item in tags)
            {
                EntityStatus status = item.Status.ParseEnum<EntityStatus>();
                if (!status.Equals(EntityStatus.I))
                {
                    if (!tagDictionary.ContainsKey(status))
                    {
                        tagDictionary.Add(status, new List<LessonTag>());
                    }
                    tagDictionary[status].Add(
                            new LessonTag()
                            {
                                LessonTagName = item.LessonTagName
                            }
                        );
                }
            }
            return tagDictionary;
        }


        //public static ILookup<EntityState, LessonFeedback> ToLessonFeedbacks(this ICollection<FeedbackViewModel> feedBacks)
        //{
        //    // Create a Lookup to organize the packages. Use the first character of Company as the key value.
        //    // Select Company appended to TrackingNumber for each element value in the Lookup.
        //    Lookup<char, string> lookup = (Lookup<char, string>)feedBacks.ToLookup(p => Convert.ToChar(p.Company.Substring(0, 1)),
        //                                                    p => p.Company + " " + p.TrackingNumber);

        //    ILookup<EntityState, LessonFeedback> _feedbacks = (Lookup<EntityState, LessonFeedback>)feedBacks
        //        .ToLookup(
        //        p => p.Status.ParseEnum<EntityState>(), 
        //        p => new LessonFeedback() { 
        //            Id = p.LessonFeedbackId == null ? Guid.NewGuid() : 
        //            Feedback = p.Feedback,
        //            Nature = p.Nature
        //        }
        //        );
        //    foreach (var item in feedBacks.Where(f => !f.Status.Equals(Constants.LESSON_FEEDBACK_REMOVED)))
        //    {
        //        LessonFeedback fb = new LessonFeedback()
        //        {
        //            Feedback = item.Feedback,
        //            Id = item.LessonFeedbackId ?? 0,
        //            Nature = item.Nature
        //        };
        //        _feedbacks.Add(fb);
        //    }
        //    return _feedbacks;
        //}

        //public static ICollection<LessonTag> ToLessonTags(this ICollection<TagViewModel> tags)
        //{
        //    ICollection<LessonTag> _tags = new HashSet<LessonTag>();
        //    foreach (var item in tags.Where(t => !t.Status.Equals(Constants.LESSON_TAG_REMOVED)))
        //    {
        //        LessonTag tag = new LessonTag()
        //        {
        //            LessonTagName = item.LessonTagName
        //        };
        //        _tags.Add(tag);
        //    }
        //    return _tags;
        //}


    }

    public class CommentViewModel
    {
        public int? Id { get; set; }
        [Required]
        public int LessonId { get; set; }
        [Required]
        public UserViewModel Author { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime? Date { get; set; }
        public int? ParentId { get; set; }
        [Required]
        public int Level { get; set; }
        //[Required]
        //public int Vers { get; set; }
    }

    public class RatingViewModel
    {
        public int? Id { get; set; }
        [Required]
        public int LessonId { get; set; }
        [Required]
        public UserViewModel Author { get; set; }
        [Required]
        public int Rating { get; set; }
        public string Content { get; set; }
        public DateTime? Date { get; set; }
    }

}
