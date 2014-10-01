using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Discitur.QueryStack.Model
{
    [Table("dbo.IdentityMap")]
    public class IdentityMap
    {
        [Key]
        public Guid AggregateId { get; set; }
        [Required]
        public string TypeName { get; set; }
        [Required]
        public int ModelId { get; set; }
    }
}
