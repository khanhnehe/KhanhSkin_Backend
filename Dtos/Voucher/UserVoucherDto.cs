using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Share.Dtos;
using System;

namespace KhanhSkin_BackEnd.Dtos.Voucher
{
    public class UserVoucherDto : BaseDto
    {
        public Guid VoucherId { get; set; }
        public CreateUpdateVoucherDto Voucher { get; set; }
        public Guid UserId { get; set; }
        public UserDto User { get; set; }
        public bool IsUsed { get; set; } = false; // user đã save voucher hay chưa
    }
}
