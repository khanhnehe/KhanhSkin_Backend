namespace KhanhSkin_BackEnd.Dtos.Report
{
    public class CostPriceDto
    {
        public Guid? ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public decimal CostPrice { get; set; } // Giá vốn của từng lần nhập
        public int QuantityChange { get; set; } // Số lượng nhập trong lần giao dịch
    }
}
