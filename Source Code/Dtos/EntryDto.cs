using MoneyTrackr.Data.DomainObjects;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyTrackr.Dtos
{
    public class EntryDto
    {
        public int? Id { get; set; }

        [Required]
        public string Description { get; set; }
        
        [Required]
        public double? Amount { get; set; }

        [Required]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Converts the Dto to the Model Object.
        /// </summary>
        public Entry Convert(string userId)
        {
            return new Entry()
            {
                Id = Id ?? default(int),
                Date = Date ?? default(DateTime),
                Description = Description,
                Amount = Amount ?? default(double),
                UserId = userId
            };
        }

        /// <summary>
        /// Converts a Model Object into a new instance of a Dto
        /// </summary>
        public static EntryDto ConvertBack(Entry entry)
        {
            return new EntryDto()
            {
                Id = entry.Id,
                Description = entry.Description,
                Date = entry.Date,
                Amount = entry.Amount
            };
        }
    }
}
