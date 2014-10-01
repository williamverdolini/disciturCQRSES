using System;

namespace Discitur.Domain.Model
{
    public class LessonFeedback
    {
        public Guid Id { get; set; }
        //public Guid LessonId { get; set; }
        //public int LessonFeedbackId { get; set; }
        //public int LessonId { get; set; }
        public int Nature { get; set; }
        public string Feedback { get; set; }
    }
}
