using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class OrderItem : BaseEntity
    {
        [Required]
        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public Guid? VariantId { get; set; }

        [ForeignKey("VariantId")]
        public ProductVariant Variant { get; set; }

        public string ProductName { get; set; }

        public string? NameVariant { get; set; } // Tên biến thể, có thể null

        public decimal ProductPrice { get; set; } // Giá của sản phẩm hoặc biến thể
        public decimal ProductSalePrice { get; set; } // Giá bán của sản phẩm hoặc biến thể

        public IList<string> Images { get; set; } = new List<string>();

        [Required]
        public int Amount { get; set; } = 1;

        [Required]
        public decimal ItemsPrice { get; set; }

        [Required]
        public Guid OrderId { get; set; } // Khóa ngoại liên kết đến đơn hàng

        [ForeignKey("OrderId")]
        public Order Order { get; set; } // Thông tin đơn hàng liên kết
    }
}
