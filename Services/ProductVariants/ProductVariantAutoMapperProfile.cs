using AutoMapper;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Dtos.ProductVariant;

namespace KhanhSkin_BackEnd.Services.ProductVariants
{
    public class ProductVariantAutoMapperProfile : Profile
    {
        public ProductVariantAutoMapperProfile()
        {
            // Cấu hình AutoMapper cho ProductVariant
            CreateMap<ProductVariant, ProductVariantDto>();
            CreateMap<ProductVariantDto, ProductVariant>();
        }
    }
}
