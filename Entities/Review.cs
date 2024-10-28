using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class Review : BaseEntity
    {
        // Các thông tin của User
        public string UserFullName { get; set; } // Thêm FullName của User
        public string? UserImage { get; set; } // Thêm Image của User

        [Required]
        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } // Liên kết với Product

        public Guid? OrderId { get; set; } // OrderId không bắt buộc

        [ForeignKey("OrderId")]
        public Order? Order { get; set; } // Liên kết tùy chọn với Order

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao!")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = false; // Trạng thái phê duyệt
    }
}
