using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Cart
{
    public class AddProductToCartDto : BaseDto
    {
        public Guid ProductId { get; set; }
        public int AmountAdd { get; set; }
        public Guid? VariantId { get; set; }
        public Guid? VoucherId { get; set; }

    }
}
