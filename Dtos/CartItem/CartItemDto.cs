using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.CartItem
{
    public class CartItemDto : BaseDto
    {
       public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public string ProductName { get; set; }
        public string? NameVariant { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal ProductSalePrice { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public int Amount { get; set; }
        public decimal ItemsPrice { get; set; }
        public Guid CartId { get; set; }
    }
}
