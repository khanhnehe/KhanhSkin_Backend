using KhanhSkin_BackEnd.Dtos.ProductVariant;
using KhanhSkin_BackEnd.Share.Dtos;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.Product
{
    public class CreateUpdateProductDto : BaseDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm!")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm!")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm!")]
        public decimal Price { get; set; }


        [Required(ErrorMessage = "Vui lòng nhập số lượng sản phẩm!")]
        public int Quantity { get; set; }
        public int? Discount { get; set; }
        public decimal? SalePrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập sku sản phẩm!")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn brand sản phẩm!")]
        public Guid BrandId { get; set; }


        [Required(ErrorMessage = "Vui lòng chọn danh mục sản phẩm!")]
        public ICollection<Guid> CategoryIds { get; set; } = new List<Guid>();


        [Required(ErrorMessage = "Vui lòng chọn phân loại sản phẩm!")]
        public ICollection<Guid> ProductTypeIds { get; set; } = new List<Guid>();
        public IList<string> Images { get; set; } = new List<string>(); // Chuyển sang IList<string>

        // Thêm thuộc tính Variants
        public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
    }
}
