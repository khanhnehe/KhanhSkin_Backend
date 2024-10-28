using System;
using System.Collections.Generic;
using static KhanhSkin_BackEnd.Consts.Enums;
using KhanhSkin_BackEnd.Share.Dtos;

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
        public DateTime StartTime { get; set; }
        public decimal DiscountValue { get; set; }

        public int TotalUses { get; set; }
        public int UsesCount { get; set; }
        public bool IsActive { get; set; }

        // Adding the relation with ProductVouchers
        public List<CreateProductVoucherDto> ProductVouchers { get; set; } = new List<CreateProductVoucherDto>();
    }
}
