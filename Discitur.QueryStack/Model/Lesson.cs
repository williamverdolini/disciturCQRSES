using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Discitur.QueryStack.Model
{
    [Table("discitur.Lesson")]
    public class Lesson
    {
        public Lesson()
        {
            this.Tags = new List<LessonTag>();
            this.FeedBacks = new HashSet<LessonFeedback>();
        }

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
        public int Rate { get; set; }
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        virtual public User Author { get; set; }
        //[Required]
        public DateTime? PublishDate { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string Conclusion { get; set; }
        public int Published { get; set; }
        [Required]
        public DateTime? CreationDate { get; set; }
        [Required]
        public DateTime? LastModifDate { get; set; }
        [Required]
        public string LastModifUser { get; set; }
        [Required]
        public int Vers { get; set; }
        [Required]
        public int RecordState { get; set; }

        // Lazy loading of Tags
        public virtual ICollection<LessonTag> Tags { get; private set; }
        // Lazy loading of Feedbacks
        public virtual ICollection<LessonFeedback> FeedBacks { get; private set; }
        // Lazy loading of Comments
        // public virtual ICollection<LessonComment> Comments { get; private set; }
    }
}