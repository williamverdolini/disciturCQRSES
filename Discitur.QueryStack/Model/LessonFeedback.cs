using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discitur.QueryStack.Model
{
    [Table("discitur.LessonFeedback")]
    public class LessonFeedback
    {
        public int LessonFeedbackId { get; set; }
        [Required]
        public int LessonId { get; set; }
        //[ForeignKey("LessonId")]
        //public virtual Lesson Lesson { get; set; }
        [Required]
        public int Nature { get; set; }
        [Required]
        public string Feedback { get; set; }
        [NotMapped]
        public Guid LessonFeedbackGuid { get; set; }
    }
}