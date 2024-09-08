﻿using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Order : BaseEntity
{
    [Required]
    public string TrackingCode { get; set; }

    [Required]
    public Guid? UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    public Guid? CartId { get; set; }

    [ForeignKey("CartId")]
    public Cart Cart { get; set; }

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
    public Enums.ShippingMethod ShippingMethod { get; set; }

    public Enums.PaymentMethod PaymentMethod { get; set; }

    public Guid? AddressId { get; set; }

    [ForeignKey("AddressId")]
    public Address Address { get; set; }

    [Required]
    public string ShippingAddressSnapshot { get; set; }


    public DateTime OrderDate { get; set; } = DateTime.Now;

    public DateTime? DeliveryDate { get; set; }

    public Enums.OrderStatus OrderStatus { get; set; } = Enums.OrderStatus.Pending;
}