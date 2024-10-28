using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.Voucher;
using KhanhSkin_BackEnd.Entities;
using System.Linq;

namespace KhanhSkin_BackEnd.Services.Voucher
{
    public class VoucherAutoMapperProfile : Profile
    {
        public VoucherAutoMapperProfile()
        {
            // Ánh xạ giữa thực thể Voucher và VoucherDto
            CreateMap<KhanhSkin_BackEnd.Entities.Voucher, VoucherDto>()
                .ForMember(dest => dest.ProductVouchers, opt => opt.MapFrom(src => src.ProductVouchers));

            // Ánh xạ ngược từ VoucherDto sang thực thể Voucher
            CreateMap<VoucherDto, KhanhSkin_BackEnd.Entities.Voucher>()
                .ForMember(dest => dest.ProductVouchers, opt => opt.Ignore());

            // Ánh xạ giữa thực thể Voucher và CreateUpdateVoucherDto
            CreateMap<KhanhSkin_BackEnd.Entities.Voucher, CreateUpdateVoucherDto>()
                .ForMember(dest => dest.ProductVouchers, opt => opt.MapFrom(src => src.ProductVouchers.Select(pv => new ProductVoucherDto
                {
                    ProductId = pv.ProductId,
                    VoucherId = pv.VoucherId,
                    Product = new ProductDto
                    {
                        Id = pv.Product.Id,
                        ProductName = pv.Product.ProductName,
                        Price = pv.Product.Price,
                        Quantity = pv.Product.Quantity,
                        SalePrice = pv.Product.SalePrice,
                        SKU = pv.Product.SKU,
                        Images = pv.Product.Images != null && pv.Product.Images.Any()
                            ? new List<string> { pv.Product.Images.First() }
                            : new List<string>() // Lấy phần tử đầu tiên của images nếu tồn tại
                    }
                }).ToList()));

            // Ánh xạ ngược từ CreateUpdateVoucherDto sang thực thể Voucher
            CreateMap<CreateUpdateVoucherDto, KhanhSkin_BackEnd.Entities.Voucher>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.ProductVouchers, opt => opt.Ignore())
             .AfterMap((src, dest) =>
             {
                 if (src.ProductVouchers != null && src.ProductVouchers.Any())
                 {
                     dest.ProductVouchers = src.ProductVouchers.Select(pv => new ProductVoucher
                     {
                         ProductId = pv.ProductId,
                         VoucherId = dest.Id,
                         Id = Guid.NewGuid()
                     }).ToList();
                 }
             });

            // Ánh xạ giữa thực thể ProductVoucher và ProductVoucherDto
            CreateMap<ProductVoucher, ProductVoucherDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.VoucherId, opt => opt.MapFrom(src => src.VoucherId))
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => new ProductDto
                {
                    Id = src.Product.Id,
                    ProductName = src.Product.ProductName,
                    Price = src.Product.Price,
                    Quantity = src.Product.Quantity,
                    SalePrice = src.Product.SalePrice,
                    SKU = src.Product.SKU,
                    Images = src.Product.Images != null && src.Product.Images.Any()
                        ? new List<string> { src.Product.Images.First() }
                        : new List<string>()
                }));

            // Ánh xạ giữa thực thể ProductVoucher và CreateProductVoucherDto
            CreateMap<ProductVoucher, CreateProductVoucherDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

            // Ánh xạ ngược từ CreateProductVoucherDto sang thực thể ProductVoucher
            CreateMap<CreateProductVoucherDto, ProductVoucher>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VoucherId, opt => opt.Ignore());

            // Ánh xạ giữa thực thể Product và ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.SalePrice, opt => opt.MapFrom(src => src.SalePrice))
                .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.SKU))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null && src.Images.Any()
                    ? new List<string> { src.Images.First() }
                    : new List<string>()));
        }
    }
}
