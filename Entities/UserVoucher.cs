﻿using KhanhSkin_BackEnd.Consts;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class UserVoucher : BaseEntity
    {
        [ForeignKey("VoucherId")]
        public Guid VoucherId { get; set; }
        public Voucher Voucher { get; set; }

        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        public User User { get; set; }

        public bool IsUsed { get; set; } = false; // User đã lưu voucher hay chưa

        public int UsageCount { get; set; } // Số lần user đã sử dụng voucher

    }
}
