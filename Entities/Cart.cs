using KhanhSkin_BackEnd.Consts;
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

        
    }
}
