using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Voucher
{
    public class ProductVoucherDto : BaseDto
    {
        public Guid ProductId { get; set; }
        public Guid VoucherId { get; set; }
        public ProductDto Product { get; set; } // Thêm thuộc tính Product

    }
}
