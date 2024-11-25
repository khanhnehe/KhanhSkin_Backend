namespace KhanhSkin_BackEnd.Dtos.Report
{
    public class TopProductReportDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
