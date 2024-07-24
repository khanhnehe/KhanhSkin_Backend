using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.ProductVariant
{
    public class ProductVariantDto : BaseDto
    {
        public string NameVariant { get; set; }
        public decimal PriceVariant { get; set; }
        public int QuantityVariant { get; set; }
        public int? DiscountVariant { get; set; }
        public decimal? SalePriceVariant { get; set; }
        public string SKUVariant { get; set; }
        public string ImageUrl { get; set; } // Bổ sung thuộc tính ImageUrl
        public Guid ProductId { get; set; } // Include if you need to reference the parent product
    }
}
