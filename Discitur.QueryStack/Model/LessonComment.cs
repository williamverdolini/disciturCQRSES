using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Discitur.QueryStack.Model
{
    [Table("discitur.Comment")]
    public class LessonComment
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int LessonId { get; set; }
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        virtual public User Author { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public int? ParentId { get; set; }
        [Required]
        public int Level { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public DateTime LastModifDate { get; set; }
        [Required]
        public string LastModifUser { get; set; }
        [Required]
        public int Vers { get; set; }
        [Required]
        public int RecordState { get; set; }

    }
}