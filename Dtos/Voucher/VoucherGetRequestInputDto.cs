using KhanhSkin_BackEnd.Share.Dtos;
using static KhanhSkin_BackEnd.Consts.Enums;

namespace KhanhSkin_BackEnd.Dtos.Voucher
{
    public class VoucherGetRequestInputDto: BaseGetRequestInput
    {
        public VoucherType? VoucherType { get; set; }

        public string? Status { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
