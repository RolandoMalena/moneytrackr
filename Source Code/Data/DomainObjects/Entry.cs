using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrackr.Data.DomainObjects
{
    public class Entry
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        public double Amount { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}
