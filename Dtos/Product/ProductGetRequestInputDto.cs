using KhanhSkin_BackEnd.Share.Dtos;

public class ProductGetRequestInputDto : BaseGetRequestInput
{
    public Guid? BrandId { get; set; } // Lọc theo BrandId
    public List<Guid> CategoryIds { get; set; } // Lọc theo danh sách CategoryIds
    public List<Guid> ProductTypeIds { get; set; } // Lọc theo danh sách ProductTypeIds
    public decimal? MinPrice { get; set; } // Giá thấp nhất
    public decimal? MaxPrice { get; set; } // Giá cao nhất
    public string? SortBy { get; set; } // Tham số sắp xếp (bán chạy nhất, sản phẩm mới nhất, giá cao/thấp)
    public bool IsAscending { get; set; } = true; // Quyết định sắp xếp tăng hay giảm

    public ProductGetRequestInputDto()
    {
        CategoryIds = new List<Guid>();
        ProductTypeIds = new List<Guid>();
    }
}
