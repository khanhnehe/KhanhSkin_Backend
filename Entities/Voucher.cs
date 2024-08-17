using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static KhanhSkin_BackEnd.Consts.Enums;
using KhanhSkin_BackEnd.Dtos.Product;

namespace KhanhSkin_BackEnd.Entities
{
    public class Voucher : BaseEntity
    {
        [Required(ErrorMessage = "Vui lòng nhập tên chương trình giảm giá!")]
        [StringLength(100, ErrorMessage = "Tên chương trình giảm giá không được quá 100 ký tự.")]
        public string ProgramName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã voucher!")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "Mã voucher phải từ 1 đến 5 ký tự và chỉ bao gồm chữ cái hoặc số.")]
        public string Code { get; set; }

        public VoucherType VoucherType { get; set; }

       

        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá trị đơn hàng tối thiểu!")]
        public decimal MinimumOrderValue { get; set; }

        public DateTime EndTime { get; set; } // Chỉ giữ ngày hết hạn

        public int TotalUses { get; set; }
        public int UsesCount { get; set; } // Số lần đã sử dụng
        public bool IsActive { get; set; } // Trạng thái hoạt động của voucher

        public ICollection<Product> ApplicableProducts { get; set; }// Sử dụng quan hệ trực tiếp

        public ICollection<UserVoucher> UserVouchers { get; set; } // Quan hệ với UserVoucher

        public Voucher()
        {
            ApplicableProducts = new HashSet<Product>();
            UserVouchers = new HashSet<UserVoucher>();
            IsActive = true; // Khởi tạo voucher với trạng thái hoạt động
        }
    }
}
