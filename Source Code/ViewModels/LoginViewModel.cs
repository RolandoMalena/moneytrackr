using System.ComponentModel.DataAnnotations;

namespace MoneyTrackr.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(25, MinimumLength = 5, ErrorMessage = "The Username must be between {2} and {1} characters long.")]
        public string Username { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 5, ErrorMessage = "The Password must be between {2} and {1} characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
