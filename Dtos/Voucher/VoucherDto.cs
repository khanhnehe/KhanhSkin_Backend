using System;
using System.Collections.Generic;
using static KhanhSkin_BackEnd.Consts.Enums;
using KhanhSkin_BackEnd.Share.Dtos;
using KhanhSkin_BackEnd.Consts;

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
        public decimal DiscountValue { get; set; }

        public int TotalUses { get; set; }
        public int UsesCount { get; set; }
        public bool IsActive { get; set; }


        // Adding the relation with ProductVouchers
        public ICollection<ProductVoucherDto> ProductVouchers { get; set; } = new List<ProductVoucherDto>();
    }
}
