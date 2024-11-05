using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Dtos.Product; // Giả sử ProductDto được định nghĩa trong Dtos.Product
using KhanhSkin_BackEnd.Share.Dtos;
using System;

namespace KhanhSkin_BackEnd.Dtos.Review
{
    public class ReviewDto : BaseDto
    {
        // Liên kết với User
        public Guid UserId { get; set; }
        public UserDto User { get; set; } // Chứa thông tin chi tiết về User

        // Liên kết với Product
        public Guid ProductId { get; set; }
        public ProductDto Product { get; set; } // Chứa thông tin chi tiết về Product
        public Guid? VariantId { get; set; } // Thêm VariantId cho biến thể sản phẩm

        // Thông tin đánh giá
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsApproved { get; set; }

        public bool HasReviewed { get; set; }
    }
}
