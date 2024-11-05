using KhanhSkin_BackEnd.Consts;
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


    [Required]
    public string PhoneNumber { get; set; }

    [Required]
    public string Province { get; set; }

    [Required]
    public string District { get; set; }

    [Required]
    public string Ward { get; set; }

    [Required]
    public string AddressDetail { get; set; }
    public string? Note { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    public DateTime? DeliveryDate { get; set; }

    public bool HasReviewe { get; set; } = false;

    public Enums.OrderStatus OrderStatus { get; set; } = Enums.OrderStatus.Pending;
}
