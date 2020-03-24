namespace MoneyTrackr.Dtos
{
    public class ReportDto
    {
        public double CurrentBalance { get; set; } = 0;
        public int EntryCount { get; set; } = 0;

        public double TotalDeposits { get; set; } = 0;
        public double DepositAverage { get; set; } = 0;
        public int DepositCount { get; set; } = 0;

        public double TotalWithdraws { get; set; } = 0;
        public double WithdrawAverage { get; set; } = 0;
        public int WithdrawCount { get; set; } = 0;

        public ReportDetailDto[] Entries { get; set; }
    }

    public class ReportDetailDto : EntryDto
    {
        public ReportDetailDto()
        {
        }

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
