using KhanhSkin_BackEnd.Dtos.ProductVariant;

namespace KhanhSkin_BackEnd.Dtos.Product
{
    public class ProductOutstandingDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int? Discount { get; set; }
        public decimal? SalePrice { get; set; }
        public string SKU { get; set; }
        public int Purchases { get; set; }
        public decimal AverageRating { get; set; }
        public IList<string> Images { get; set; }
    }
}
