using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.ProductVariant;
using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Dtos.Category;
using KhanhSkin_BackEnd.Dtos.ProductType;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Products
{
    public class ProductAutoMapperProfile : Profile
    {
        public ProductAutoMapperProfile()
        {
            // Ánh xạ từ thực thể Product sang ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => new BrandDto { Id = src.Brand.Id, BrandName = src.Brand.BrandName }))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => new CategoryDto { Id = c.Id, CategoryName = c.CategoryName }).ToList()))
                .ForMember(dest => dest.ProductTypes, opt => opt.MapFrom(src => src.ProductTypes.Select(pt => new ProductTypeDto { Id = pt.Id, TypeName = pt.TypeName }).ToList()))
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.ToList())); // Chuyển đổi Images sang IList<string>

            // Ánh xạ từ thực thể ProductVariant sang ProductVariantDto
            CreateMap<ProductVariant, ProductVariantDto>();

            // Ánh xạ từ thực thể Brand sang BrandDto
            CreateMap<Brand, BrandDto>();

            // Ánh xạ từ thực thể Category sang CategoryDto
            CreateMap<Category, CategoryDto>();

            // Ánh xạ từ thực thể ProductType sang ProductTypeDto
            CreateMap<ProductType, ProductTypeDto>();

            // Ánh xạ từ CreateUpdateProductDto sang Product
            CreateMap<CreateUpdateProductDto, Product>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore()) // Thêm dòng này để loại trừ Id
                .ForMember(dest => dest.Variants, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.ProductTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.ToList())); // Chuyển đổi Images sang IList<string>
        }
    }
}
