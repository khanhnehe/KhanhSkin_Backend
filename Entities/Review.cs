using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class Review : BaseEntity
    {
        // Thông tin người dùng
        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        // Thông tin sản phẩm
        [Required]
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        // Thông tin biến thể (nếu có)
        public Guid? VariantId { get; set; } // Tham chiếu đến biến thể của sản phẩm
        [ForeignKey("VariantId")]
        public ProductVariant Variant { get; set; } // Liên kết với biến thể sản phẩm

        // Thông tin đơn hàng (nếu có)
        public Guid? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        // Thông tin đánh giá
        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao!")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = false; // Trạng thái phê duyệt
        public bool HasReviewed { get; set; } = false; // Đặt mặc định là false
    }
}
