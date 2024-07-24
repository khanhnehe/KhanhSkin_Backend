using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.ProductVariant;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.ProductVariants
{
    public interface IProductVariantService : IBaseService<ProductVariant, ProductVariantDto, ProductVariantDto, ProductVariantGetRequestInputDto>
    {
    
    }
}
