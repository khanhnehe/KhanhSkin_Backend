using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Dtos.ProductType;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Brands
{
    public interface IBrandService : IBaseService<Brand, BrandDto, BrandDto, BrandGetRequestInputDto>
    {

        Task<List<BrandDto>> Search(string brandName);

    }
}
