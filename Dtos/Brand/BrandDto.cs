using KhanhSkin_BackEnd.Share.Dtos;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.Brand
{
    public class BrandDto : BaseDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên thương hiệu!")]
        public string BrandName { get; set; }

        // Sử dụng DTO cho Product nếu bạn muốn trả về thông tin sản phẩm liên quan
        // Điều này giúp tránh việc trả về quá nhiều thông tin không cần thiết và tăng tính bảo mật
        // public ICollection<ProductDto> Products { get; set; }
    }
}

