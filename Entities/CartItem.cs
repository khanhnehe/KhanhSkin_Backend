using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class CartItem : BaseEntity
    {

        [Required]

        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public Guid? VariantId { get; set; }

        [ForeignKey("VariantId")]
        public ProductVariant Variant { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm!")]
        public string ProductName { get; set; }

        public string? NameVariant { get; set; } // Tên biến thể, có thể null

        public decimal ProductPrice { get; set; } // Giá của sản phẩm hoặc biến thể
        public decimal ProductSalePrice { get; set; } // Giá bán của sản phẩm hoặc biến thể

        public IList<string> Images { get; set; } = new List<string>();

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0!")]
        public int Amount { get; set; } = 1;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá mục phải lớn hơn hoặc bằng 0!")]
        public decimal ItemsPrice { get; set; }

        [Required]
        public Guid CartId { get; set; } // Khóa ngoại liên kết đến giỏ hàng

        [ForeignKey("CartId")]
        public Cart Cart { get; set; } // Thông tin giỏ hàng liên kết

    }
}
