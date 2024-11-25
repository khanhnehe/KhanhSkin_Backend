using KhanhSkin_BackEnd.Consts;

namespace KhanhSkin_BackEnd.Dtos.Report
{
    public class ReportRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Enums.PeriodType PeriodType { get; set; } // Sử dụng Enum PeriodType
    }
}
