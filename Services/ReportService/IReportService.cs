using KhanhSkin_BackEnd.Dtos.Report;

namespace KhanhSkin_BackEnd.Services.ReportService
{
    public interface IReportService
    {
        Task<List<ReportResponseDto>> GetReportAsync(ReportRequestDto request);

    }
}
