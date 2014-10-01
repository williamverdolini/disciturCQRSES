using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discitur.QueryStack.Model
{
    [Table("discitur.UserActivation")]
    public class UserActivation
    {
        [Key]
        public string UserName { get; set; }
        [Required]
        public string Key { get; set; }

    }
}