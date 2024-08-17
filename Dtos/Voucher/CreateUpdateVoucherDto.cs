using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Share.Dtos;
using System;
using System.Collections.Generic;
using static KhanhSkin_BackEnd.Consts.Enums;

namespace KhanhSkin_BackEnd.Dtos.Voucher
{
    public class CreateUpdateVoucherDto : BaseDto
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

        // Chỉ giữ ApplicableProductIds để lưu danh sách ID của các sản phẩm mà voucher áp dụng
        public ICollection<Guid> ApplicableProductIds { get; set; }


        public CreateUpdateVoucherDto()
        {
            ApplicableProductIds = new HashSet<Guid>();
        }
    }
}
