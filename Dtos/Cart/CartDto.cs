using KhanhSkin_BackEnd.Dtos.CartItem;
using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Cart
{
    public class CartDto : BaseDto
    {
        public Guid UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    }
}
