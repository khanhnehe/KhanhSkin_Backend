using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Category;
using KhanhSkin_BackEnd.Dtos.ProductType; // Đảm bảo bạn đã thêm using này
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
                .ForMember(dest => dest.ProductIds, opt => opt.MapFrom(src => src.Products.Select(p => p.Id)))
                .ForMember(dest => dest.ProductTypes, opt => opt.MapFrom(src => src.ProductTypes)); // Thêm dòng này

            // Ánh xạ từ DTO sang Entity
            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.ProductTypes, opt => opt.Ignore()) // Bỏ qua vì cần xử lý thủ công
                .ForMember(dest => dest.Products, opt => opt.Ignore()); // Bỏ qua vì cần xử lý thủ công

            // Đảm bảo bạn đã cấu hình ánh xạ từ ProductType sang ProductTypeDto
            CreateMap<ProductType, ProductTypeDto>();
        }
    }
}
