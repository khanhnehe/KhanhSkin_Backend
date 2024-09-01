using KhanhSkin_BackEnd.Consts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class Order : BaseEntity
    {
        [Required]
        public string TrackingCode { get; set; } // Mã vận đơn


        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public Guid? VoucherId { get; set; }

        [ForeignKey("VoucherId")]
        public Voucher Voucher { get; set; }

        public decimal DiscountValue { get; set; } = 0;

        [Required]
        public decimal ShippingPrice { get; set; }


        [Required]
        public decimal TotalPrice { get; set; }

        public decimal FinalPrice { get; set; } = 0;


        [Required]
        public string ShippingMethod { get; set; }

        public Guid AddressId { get; set; }

        [ForeignKey("AddressId")]
        public Address Address { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now; // Thời gian tạo đơn

        public DateTime? DeliveryDate { get; set; } // Thời gian nhận hàng (có thể null cho đến khi đơn hàng được giao)

        public Enums.OrderStatus OrderStatus { get; set; } = Enums.OrderStatus.Pending;
    }
}
