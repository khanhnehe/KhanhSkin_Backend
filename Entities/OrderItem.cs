using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class OrderItem : BaseEntity
    {
        public Guid? CartItemId { get; set; }

        [ForeignKey("CartItemId")]
        public CartItem CartItem { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public Guid? VariantId { get; set; }

        [ForeignKey("VariantId")]
        public ProductVariant Variant { get; set; }

        [Required]
        public string ProductName { get; set; } // Lưu trữ tên sản phẩm

        public string? NameVariant { get; set; } // Lưu trữ tên biến thể

        [Required]
        public decimal ProductPrice { get; set; } // Lưu trữ giá sản phẩm

        public decimal ProductSalePrice { get; set; } // Lưu trữ giá bán của sản phẩm

        public IList<string> Images { get; set; } = new List<string>(); // Lưu trữ hình ảnh sản phẩm

        [Required]
        public int Amount { get; set; } = 1; // Lưu trữ số lượng sản phẩm

        [Required]
        public decimal ItemsPrice { get; set; } // Lưu trữ tổng giá trị của OrderItem

        [Required]
        public Guid? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }
    }

}
