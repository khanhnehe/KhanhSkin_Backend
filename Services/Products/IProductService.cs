using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Services.Products
{
    public interface IProductService : IBaseService<Product, ProductDto, CreateUpdateProductDto, ProductGetRequestInputDto>
    {
        Task<List<ProductDto>> Search(string keyword);
        Task<List<ProductOutstandingDto>> GetByCategory(Guid categoryId);
        Task<List<ProductOutstandingDto>> GetByProductType(Guid productTypeId);
        Task<List<ProductOutstandingDto>> GetByBrand(Guid brandId);
        Task UpdateProductAverageRating(Guid productId);
        Task<List<ProductDto>> GetFilteredProducts(ProductGetRequestInputDto input);

    }
}
