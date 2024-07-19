namespace KhanhSkin_BackEnd.Dtos.ProductVariant
{
    public class ProductVariantDto
    {
        public Guid Id { get; set; } // Assuming BaseEntity includes an Id property
        public string NameVariant { get; set; }
        public decimal PriceVariant { get; set; }
        public int QuantityVariant { get; set; }
        public int? DiscountVariant { get; set; }
        public decimal? SalePriceVariant { get; set; }
        public string SKUVariant { get; set; }
        public Guid ProductId { get; set; } // Include if you need to reference the parent product
        
    }
}
