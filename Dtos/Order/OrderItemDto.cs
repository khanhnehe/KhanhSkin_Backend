using KhanhSkin_BackEnd.Share.Dtos;
using System;
using System.Collections.Generic;

namespace KhanhSkin_BackEnd.Dtos.Order
{
    public class OrderItemDto : BaseDto
    {
        public Guid CartItemId { get; set; }

        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public Guid? VariantId { get; set; } 

        public string? NameVariant { get; set; } 

        public decimal ProductPrice { get; set; } 

        public decimal ProductSalePrice { get; set; } 

        public int Amount { get; set; }

        public decimal ItemsPrice { get; set; } 

        public IList<string> Images { get; set; } = new List<string>(); 
    }
}
