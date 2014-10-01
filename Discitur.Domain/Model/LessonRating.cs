using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discitur.Domain.Model
{
    public class LessonRating
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public Guid UserId { get; set; }
        //public int Id { get; set; }
        //public int LessonId { get; set; }
        //public int UserId { get; set; }
        //virtual public User Author { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifDate { get; set; }
        public string LastModifUser { get; set; }
        public int Vers { get; set; }
        public int RecordState { get; set; }
    }
}
