using System.ComponentModel.DataAnnotations;

namespace Sammaron.Server.ViewModels
{
    public class PasswordLessModel
    {
        [Required]
        [RegularExpression(@"((\+?|0{2})([1-9]{1,3})|0{0,1})\d{9}")]
        public string PhoneNumber { get; set; }
        
        [RegularExpression(@"\d{6}")]
        public string Token { get; set; }
    }
}