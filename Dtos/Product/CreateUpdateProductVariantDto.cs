using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.Product
{
    public class CreateUpdateProductVariantDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên biến thể!")]
        public string NameVariant { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá biến thể!")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá biến thể phải lớn hơn hoặc bằng 0!")]
        public decimal PriceVariant { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng biến thể!")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng biến thể phải lớn hơn hoặc bằng 0!")]
        public int QuantityVariant { get; set; }

        [Range(0, 100, ErrorMessage = "Giảm giá phải nằm trong khoảng từ 0 đến 100!")]
        public int? DiscountVariant { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập SKU của biến thể!")]
        public string SKUVariant { get; set; }

        [Required(ErrorMessage = "Vui lòng thêm ảnh của biến thể!")]
        [Url(ErrorMessage = "Đường dẫn ảnh không hợp lệ!")]
        public string ImageUrl { get; set; }

        // Không bao gồm SalePriceVariant vì nó sẽ được tính toán tự động
        // Không bao gồm ProductId vì nó sẽ được xử lý bởi logic nghiệp vụ
    }
}
