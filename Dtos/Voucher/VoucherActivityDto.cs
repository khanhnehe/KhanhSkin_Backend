using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Share.Dtos;
using System;

namespace KhanhSkin_BackEnd.Dtos.Voucher
{
    public class VoucherActivityDto : BaseDto
    {
        public Guid VoucherId { get; set; }
        public CreateUpdateVoucherDto Voucher { get; set; }
        public Guid UserId { get; set; }
        public UserDto User { get; set; }
        public DateTime UsedAt { get; set; }
        public bool IsSuccess { get; set; }
    }
}
