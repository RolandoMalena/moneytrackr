using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrackr.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 5, ErrorMessage = "The Username must be between {2} and {1} characters long.")]
        public string Username { get; set; }

        [StringLength(25, MinimumLength = 5, ErrorMessage = "The Password must be between {2} and {1} characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Role")]
        [Required]
        public string RoleId { get; set; }

        public List<RoleViewModel> Roles { get; set; }
    }
}