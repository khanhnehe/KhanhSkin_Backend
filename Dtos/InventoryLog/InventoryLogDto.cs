using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Share.Dtos;
using System;

namespace KhanhSkin_BackEnd.Dtos.InventoryLog
{
    public class InventoryLogDto : BaseDto
    {
        public Guid? ProductId { get; set; }             // ID của sản phẩm (chỉ để lưu, không liên kết)
        public string ProductName { get; set; }          // Tên sản phẩm tại thời điểm giao dịch
        public string ProductSKU { get; set; }           // Mã SKU của sản phẩm

        public Guid? ProductVariantId { get; set; }      // ID của biến thể sản phẩm (chỉ để lưu, không liên kết)
        public string VariantName { get; set; }          // Tên biến thể tại thời điểm giao dịch
        public string VariantSKU { get; set; }           // SKU của biến thể

        public int QuantityChange { get; set; }          // Số lượng thay đổi (dương cho nhập, âm cho xuất)
        public DateTime TransactionDate { get; set; }    // Ngày thực hiện giao dịch
        public Enums.ActionType ActionType { get; set; } // Loại hành động: Nhập hoặc Xuất

        public Guid? SupplierId { get; set; }            // ID của nhà cung cấp (chỉ để lưu, không liên kết)
        public string SupplierName { get; set; }         // Tên nhà cung cấp tại thời điểm giao dịch

        public decimal? CostPrice { get; set; }          // Giá vốn khi nhập kho
        public decimal? ItemPrice { get; set; }          // Giá của từng sản phẩm
        public decimal? TotalPrice { get; set; }         // Tổng giá trị (ItemPrice * QuantityChange)

        public string CodeInventory { get; set; }        // Mã phiếu nhập kho
        public string ProductImage { get; set; }      // URL ảnh của sản phẩm
        public string VariantImage { get; set; }      // URL ảnh của biến thể

        public string Note { get; set; }                 // Ghi chú bổ sung nếu cần
    }
}
