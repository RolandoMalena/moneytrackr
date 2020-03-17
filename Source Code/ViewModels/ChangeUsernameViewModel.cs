using System.ComponentModel.DataAnnotations;

namespace MoneyTrackr.ViewModels
{
    public class ChangeUsernameViewModel
    {
        [Required]
        [StringLength(25, MinimumLength = 5, ErrorMessage = "The Username must be between {2} and {1} characters long.")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }
    }
}
