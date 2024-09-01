using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Share.Dtos;
using System;

namespace KhanhSkin_BackEnd.Dtos.Voucher
{
    public class UserVoucherDto : BaseDto
    {
        public Guid VoucherId { get; set; } // ID của voucher
        public Guid UserId { get; set; } // ID của người dùng

        public bool IsUsed { get; set; } = false; // Đánh dấu voucher đã được sử dụng hay chưa

        public int UsageCount { get; set; } // Số lần user đã sử dụng voucher

    }
}
