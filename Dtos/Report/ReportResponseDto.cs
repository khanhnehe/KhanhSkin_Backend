namespace KhanhSkin_BackEnd.Dtos.Report
{
    public class ReportResponseDto
    {
        public string TimePeriod { get; set; }
        public decimal Revenue { get; set; } // Doanh thu
        public decimal GrossProfit { get; set; } // Lợi nhuận gộp
        public decimal? GrowthRate { get; set; } // Tăng trưởng (%)
    }
}
