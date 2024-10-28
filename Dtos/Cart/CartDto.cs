using KhanhSkin_BackEnd.Dtos.CartItem;
using KhanhSkin_BackEnd.Dtos.Voucher;
using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Cart
{
    public class CartDto : BaseDto
    {
        public Guid UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();

        public Guid? VoucherId { get; set; }
        public string VoucherCode { get; set; } // Thêm thuộc tính Code của Voucher
        public string VoucherProgramName { get; set; } // Thêm thuộc tính ProgramName của Voucher
        public decimal VoucherDiscountValue { get; set; } = 0; // Thêm thuộc tính DiscountValue của Voucher

        public decimal DiscountValue { get; set; } = 0;
        public decimal FinalPrice { get; set; } = 0;
    }
}
