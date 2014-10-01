using System.ComponentModel.DataAnnotations;

namespace Discitur.CommandStack.ViewModel
{
    public class RegisterUserViewModel
    {
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        public string ActivationKey { get; set; }
    }

    public class ActivateUserViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Key { get; set; }
    }

    public class UpdateUserViewModel
    {
        [Required]
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
    }

    public class ChangeUserEmailViewModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
    }

    public class ChangeUserPictureViewModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public byte[] Picture { get; set; }
    }

}
