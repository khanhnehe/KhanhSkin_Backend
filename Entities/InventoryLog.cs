using KhanhSkin_BackEnd.Consts;
using System;

namespace KhanhSkin_BackEnd.Entities
{
    public class InventoryLog : BaseEntity
    {
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductSKU { get; set; }

        public Guid? ProductVariantId { get; set; }
        public string? VariantName { get; set; }
        public string? VariantSKU { get; set; }

        public int QuantityChange { get; set; }
        public DateTime TransactionDate { get; set; }
        public Enums.ActionType ActionType { get; set; }

        public Guid? SupplierId { get; set; }
        public string? SupplierName { get; set; }

        public decimal? CostPrice { get; set; }
        public decimal? ItemPrice { get; set; }
        public decimal? TotalPrice { get; set; }

        public string Note { get; set; }

        public string CodeInventory { get; set; } // Mã phiếu nhập kho
        public string? ProductImage { get; set; } // URL ảnh của sản phẩm
        public string? VariantImage { get; set; } // URL ảnh của biến thể
    }
}
