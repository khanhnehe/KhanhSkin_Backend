namespace KhanhSkin_BackEnd.Dtos.Product
{
    public class ProductInventoryImportDto
    {
        public Guid ProductId { get; set; } // ID của sản phẩm
        public Guid? ProductVariantId { get; set; } // ID của biến thể sản phẩm (nếu có)
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; } // Giá vốn
        public string Note { get; set; } // Ghi chú (nếu cần)
        public Guid? SupplierId { get; set; } // Nhà cung cấp (nếu có)
    }
}
