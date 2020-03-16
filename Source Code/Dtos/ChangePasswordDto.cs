﻿using System.ComponentModel.DataAnnotations;

namespace MoneyTrackr.Dtos
{
    public class ChangePasswordDto
    {
        [Required]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 5, ErrorMessage = "The Password must be between {2} and {1} characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
    }
}