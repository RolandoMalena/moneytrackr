using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrackr.ViewModels
{
    public class FilterEntriesViewModel
    {
        public FilterEntriesViewModel()
        {
            DateTime today = DateTime.Today;

            From = new DateTime(today.Year, today.Month, 1);
            To = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            Users = new List<UserViewModel>();
        }

        [Display(Name = "User")]
        public List<UserViewModel> Users { get; set; }

        [DataType(DataType.Date)]
        public DateTime? From { get; set; }

        [DataType(DataType.Date)]
        public DateTime? To { get; set; }
    }
}
