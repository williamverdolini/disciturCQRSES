using System;

namespace Discitur.Domain.Model
{
    public class LessonComment
    {
        //public int Id { get; set; }
        //public int LessonId { get; set; }
        //public int? ParentId { get; set; }
        //public int UserId { get; set; }
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid AuthorId { get; set; }
        //public Guid UserId { get; set; }
        //virtual public User Author { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int Level { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifDate { get; set; }
        public string LastModifUser { get; set; }
        public int Vers { get; set; }
        public int RecordState { get; set; }
    }
}
