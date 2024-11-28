using KhanhSkin_BackEnd.Consts;

namespace KhanhSkin_BackEnd.Dtos.Report
{
    public class ReportRequestDto
    {
        public Enums.PeriodType PeriodType { get; set; } // Xác định loại thống kê: Ngày, Tháng, Năm
        public int? SelectedMonth { get; set; } // Chỉ sử dụng khi PeriodType = Month
    }
}
