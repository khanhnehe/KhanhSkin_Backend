using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static KhanhSkin_BackEnd.Consts.Enums;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Consts;

namespace KhanhSkin_BackEnd.Entities
{
    public class Voucher : BaseEntity
    {
        [Required(ErrorMessage = "Vui lòng nhập tên chương trình giảm giá!")]
        [StringLength(100, ErrorMessage = "Tên chương trình giảm giá không được quá 100 ký tự.")]
        public string ProgramName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã voucher!")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Mã voucher phải từ 1 đến 5 ký tự và chỉ bao gồm chữ cái hoặc số.")]
        public string Code { get; set; }

        public string Description { get; set; }

        public VoucherType VoucherType { get; set; }

        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá trị đơn hàng tối thiểu!")]
        public decimal MinimumOrderValue { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm giá!")]
        public decimal DiscountValue { get; set; } // Trường để lưu giá trị giảm giá


        public DateTime EndTime { get; set; } // Chỉ giữ ngày hết hạn

        public int TotalUses { get; set; } // Sl voucher  tung ra

        public int UsesCount { get; set; } // Số lần 1 user can dùng voucher
        public bool IsActive { get; set; } // Trạng thái hoạt động của voucher


        public ICollection<ProductVoucher> ProductVouchers { get; set; } = new List<ProductVoucher>();

        public ICollection<UserVoucher> UserVouchers { get; set; } // Quan hệ với UserVoucher

        public Cart Cart { get; set; }


    }
}
