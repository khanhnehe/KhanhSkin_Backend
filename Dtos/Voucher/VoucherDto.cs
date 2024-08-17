using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Share.Dtos;
using System;
using System.Collections.Generic;
using static KhanhSkin_BackEnd.Consts.Enums;

namespace KhanhSkin_BackEnd.Dtos.Voucher
{
    public class VoucherDto : BaseDto
    {
        public string ProgramName { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        public VoucherType VoucherType { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal MinimumOrderValue { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalUses { get; set; }
        public int UsesCount { get; set; }
        public bool IsActive { get; set; }

        // Các sản phẩm áp dụng voucher này (dùng cho loại voucher áp dụng cho một số sản phẩm nhất định)
        public ICollection<Guid> ApplicableProductIds { get; set; } // Sử dụng ID để truyền dữ liệu

        public ICollection<ProductDto> ApplicableProducts { get; set; } // Bao gồm cả ProductDto để tiện truy vấn

        // Các quan hệ với UserVoucher, dùng để lưu trữ thông tin người dùng đã lưu voucher này
        public ICollection<UserVoucher> UserVouchers { get; set; }

        public VoucherDto()
        {
            ApplicableProductIds = new HashSet<Guid>();
            ApplicableProducts = new HashSet<ProductDto>();
            UserVouchers = new HashSet<UserVoucher>();
        }
    }
}
