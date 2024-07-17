using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Category;
using KhanhSkin_BackEnd.Dtos.ProductType;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Categories
{
    public class CategoryAutoMapperProfile : Profile
    {
        public CategoryAutoMapperProfile()
        {
            // Ánh xạ từ Entity sang DTO
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ProductTypeIds, opt => opt.MapFrom(src => src.ProductTypes.Select(pt => pt.Id)))
                .ForMember(dest => dest.ProductTypes, opt => opt.MapFrom(src => src.ProductTypes));

            // Ánh xạ từ DTO sang Entity
            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.ProductTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            // Ánh xạ từ ProductType sang ProductTypeDto
            //CreateMap<ProductType, ProductTypeDto>()
            //    .ForMember(dest => dest.Category, opt => opt.Ignore()); // Bỏ qua Categories để ngăn vòng lặp
        }
    }
}
