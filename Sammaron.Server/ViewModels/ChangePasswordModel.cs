using System.ComponentModel.DataAnnotations;

namespace Sammaron.Server.ViewModels
{
    public class ChangePasswordModel : PasswordModel
    {
        public string OldPassword { get; set; }
    }

    public class PasswordModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}