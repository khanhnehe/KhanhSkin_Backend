using KhanhSkin_BackEnd.Entities;

public class OrderItem : BaseEntity
{
    public Guid? CartItemId { get; set; }

    public Guid? ProductId { get; set; } // Giữ lại ProductId để sử dụng trong các thao tác sau này

    public Product Product { get; set; } // Thêm thuộc tính Product để liên kết với thực thể Product

    public Guid? VariantId { get; set; } // Giữ lại VariantId (nếu có) để sử dụng khi cập nhật số lượng

    public string ProductName { get; set; } // Lưu trữ tên sản phẩm

    public string? NameVariant { get; set; } // Lưu trữ tên biến thể (nếu có)

    public decimal ProductPrice { get; set; } // Lưu trữ giá sản phẩm

    public decimal? ProductSalePrice { get; set; } // Lưu trữ giá bán của sản phẩm (nếu có)

    public IList<string> Images { get; set; } = new List<string>(); // Lưu trữ hình ảnh sản phẩm

    public int Amount { get; set; } = 1; // Lưu trữ số lượng sản phẩm

    public decimal ItemsPrice { get; set; } // Lưu trữ tổng giá trị của OrderItem

    public Guid? OrderId { get; set; }
    public Order Order { get; set; }
}
