using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Dtos.Address;
using KhanhSkin_BackEnd.Share.Dtos;
using System;
using System.Collections.Generic;

namespace KhanhSkin_BackEnd.Dtos.Order
{
    public class OrderDto : BaseDto
    {
        public Guid UserId { get; set; }
        public string TrackingCode { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public Guid? VoucherId { get; set; }
        public decimal DiscountValue { get; set; } = 0;
        public decimal ShippingPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal FinalPrice { get; set; } = 0;
        public Enums.ShippingMethod ShippingMethod { get; set; }
        public Enums.PaymentMethod PaymentMethod { get; set; }

        public Guid AddressId { get; set; }
        public AddressDto Address { get; set; } // Thêm thuộc tính Address vào OrderDto

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? DeliveryDate { get; set; }
        public Enums.OrderStatus OrderStatus { get; set; } = Enums.OrderStatus.Pending;
    }
}
     