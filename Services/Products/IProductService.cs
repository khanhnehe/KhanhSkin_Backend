using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Products
{
    public interface IProductService : IBaseService<Product, ProductDto, CreateUpdateProductDto, ProductGetRequestInputDto>
    {
    }
}
