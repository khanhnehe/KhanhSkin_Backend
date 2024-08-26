using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class Cart : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng giá phải lớn hơn hoặc bằng 0!")]
        public decimal TotalPrice { get; set; }

        // Thêm trường VoucherId để lưu thông tin về voucher được áp dụng
        public Guid? VoucherId { get; set; }

        [ForeignKey("VoucherId")]
        public Voucher Voucher { get; set; }

        // Thêm trường để lưu giá trị giảm giá được áp dụng
        public decimal DiscountValue { get; set; } = 0;

        public decimal FinalPrice { get; set; } = 0;
    }
}
