using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discitur.QueryStack.Model
{
    [Table("discitur.User")]
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        public string Picture { get; set; }
        public string Thumb { get; set; }
        [NotMapped]
        public bool IsAdmin { get; set; }
    }

    //[NotMapped]
    //public class Account : User
    //{
    //    public string Password { get; set; }
    //}
}