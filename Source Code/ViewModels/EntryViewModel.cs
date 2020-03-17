using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrackr.ViewModels
{
    public class EntryViewModel
    {
        public int? Id { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public double? Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; } = DateTime.Today;

    }
}
