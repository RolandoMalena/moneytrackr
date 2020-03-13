using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyTrackr.Dtos
{
    public class ReportDto
    {
        public double CurrentBalance { get; set; }
        public int EntryCount { get; set; }

        public double TotalDeposits { get; set; }
        public double DepositAverage { get; set; }
        public int DepositCount { get; set; }

        public double TotalWithdraws { get; set; }
        public double WithdrawAverage { get; set; }
        public int WithdrawCount { get; set; }

        public ReportDetailDto[] Entries { get; set; }
    }

    public class ReportDetailDto : EntryDto
    {
        public ReportDetailDto(EntryDto dto)
        {
            Id = dto.Id;
            Description = dto.Description;
            Amount = dto.Amount;
            Date = dto.Date;
        }

        public double Balance { get; set; }
    }
}
