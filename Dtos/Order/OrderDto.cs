using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Dtos.Address;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Share.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.Order
{
    public class OrderDto : BaseDto
    {
        public Guid UserId { get; set; }
        public UserDto User { get; set; } // Thêm thuộc tính Address vào OrderDto

        public string TrackingCode { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public Guid? VoucherId { get; set; }
        public decimal DiscountValue { get; set; } = 0;
        public decimal ShippingPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal FinalPrice { get; set; } = 0;
        public string? Note { get; set; }

        public Enums.ShippingMethod ShippingMethod { get; set; }
        public Enums.PaymentMethod PaymentMethod { get; set; }


        public Guid AddressId { get; set; }
        public AddressDto Address { get; set; } // Thêm thuộc tính Address vào OrderDto

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? DeliveryDate { get; set; }
        public Enums.OrderStatus OrderStatus { get; set; } = Enums.OrderStatus.Pending;

        
        public string PhoneNumber { get; set; }

        public string Province { get; set; }

        public string District { get; set; }

        public string Ward { get; set; }

        public string AddressDetail { get; set; }

        public string ShippingMethodDes { get; set; }
        public string PaymentMethodDes { get; set; }
        public string OrderStatusDes { get; set; }

    }
}
     