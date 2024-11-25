using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Dtos.CartItem;
using KhanhSkin_BackEnd.Dtos.Category;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.ProductType;
using KhanhSkin_BackEnd.Dtos.ProductVariant;
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
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    NameVariant = v.NameVariant,
                    PriceVariant = v.PriceVariant,
                    QuantityVariant = v.QuantityVariant,
                    DiscountVariant = v.DiscountVariant,
                    SalePriceVariant = v.SalePriceVariant,
                    SKUVariant = v.SKUVariant,
                    ImageUrl = v.ImageUrl,
                    ProductId = v.ProductId
                }).ToList()))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.ToList()));

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
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.ProductTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.ToList()));

            // Ánh xạ từ thực thể Product sang ProductSummaryDto
            CreateMap<Product, ProductOutstandingDto>()
               //.ForMember(dest => dest.Variants, opt => opt.Ignore())
               .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => new BrandDto { Id = src.Brand.Id, BrandName = src.Brand.BrandName }))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => new CategoryDto { Id = c.Id, CategoryName = c.CategoryName }).ToList()))
                .ForMember(dest => dest.ProductTypes, opt => opt.MapFrom(src => src.ProductTypes.Select(pt => new ProductTypeDto { Id = pt.Id, TypeName = pt.TypeName }).ToList()));

            CreateMap<ProductVariantDto, CreateUpdateProductVariantDto>();
            // Ánh xạ từ CartItem sang CartItemDto
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.NameVariant, opt => opt.MapFrom(src => src.Variant != null ? src.Variant.NameVariant : null))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Variant != null ? src.Variant.PriceVariant : src.Product.Price))
                .ForMember(dest => dest.ProductSalePrice, opt => opt.MapFrom(src => src.Variant != null ? src.Variant.SalePriceVariant : src.Product.SalePrice));

            CreateMap<ProductInventoryImportDto, KhanhSkin_BackEnd.Entities.InventoryLog>()
               .ForMember(dest => dest.Id, opt => opt.Ignore());

        }
    }
}
