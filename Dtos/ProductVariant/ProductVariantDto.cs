using KhanhSkin_BackEnd.Share.Dtos;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.ProductVariant
{
    public class ProductVariantDto : BaseDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên biến thể!")]
        public string NameVariant { get; set; }
        public decimal PriceVariant { get; set; }
        public int QuantityVariant { get; set; }
        public int? DiscountVariant { get; set; }
        public decimal? SalePriceVariant { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập SKU của biến thể!")]
        public string SKUVariant { get; set; }

        [Required(ErrorMessage = "Vui lòng thêm ảnh của biến thể!")]
        public string ImageUrl { get; set; } // Bổ sung thuộc tính ImageUrl
        public Guid ProductId { get; set; } // Include if you need to reference the parent product
    }
}