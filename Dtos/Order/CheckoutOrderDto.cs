using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Share.Dtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.Order
{
    public class CheckoutOrderDto : BaseDto
    {
        [Required]
        public Guid UserId { get; set; } // ID của người dùng thực hiện thanh toán

        [Required]
        public Guid CartId { get; set; } // ID của giỏ hàng

        [Required]
        public Guid AddressId { get; set; } // ID của địa chỉ giao hàng


        [Required]
        public Enums.ShippingMethod ShippingMethod { get; set; } // Phương thức vận chuyển

        [Required]
        public Enums.PaymentMethod PaymentMethod { get; set; } // Phương thức thanh toán
    }
}
