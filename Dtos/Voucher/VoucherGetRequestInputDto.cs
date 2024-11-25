using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Voucher
{
    public class VoucherGetRequestInputDto: BaseGetRequestInput
    {
        public bool? IsActive { get; set; }
        public string? Status { get; set; }

    }
}
