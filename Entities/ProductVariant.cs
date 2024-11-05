using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class ProductVariant : BaseEntity
    {

        public string NameVariant { get; set; }
        public decimal PriceVariant { get; set; }
        public int QuantityVariant { get; set; }
        public int? DiscountVariant { get; set; }
        public decimal? SalePriceVariant { get; set; }
        public string SKUVariant { get; set; }
        public string ImageUrl { get; set; }
        public bool IsDeleted { get; set; } = false;


        // Foreign key
        [ForeignKey("ProductId")]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        // Liên kết với Review
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>(); // Thêm thuộc tính CartItems

    }
}
