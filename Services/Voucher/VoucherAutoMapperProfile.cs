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
                .ForMember(dest => dest.ProductVouchers, opt => opt.Ignore()); // Ignore để tránh circular references

            // Ánh xạ giữa thực thể Voucher và CreateUpdateVoucherDto
            CreateMap<KhanhSkin_BackEnd.Entities.Voucher, CreateUpdateVoucherDto>()
                .ForMember(dest => dest.ProductVouchers, opt => opt.MapFrom(src => src.ProductVouchers.Select(pv => new ProductVoucherDto
                {
                    ProductId = pv.ProductId,
                    VoucherId = pv.VoucherId,
                    Product = new ProductDto
                    {
                        Id = pv.Product.Id,
                        // Thêm các thuộc tính khác của ProductDto nếu cần
                    }
                }).ToList()));

            // Ánh xạ ngược từ CreateUpdateVoucherDto sang thực thể Voucher
            CreateMap<CreateUpdateVoucherDto, KhanhSkin_BackEnd.Entities.Voucher>()
             .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ignore Id để tránh thay đổi giá trị của nó
             .ForMember(dest => dest.ProductVouchers, opt => opt.Ignore()) // Ignore để tránh circular references
             .AfterMap((src, dest) =>
             {
                 if (src.ProductVouchers != null && src.ProductVouchers.Any())
                 {
                     dest.ProductVouchers = src.ProductVouchers.Select(pv => new ProductVoucher
                     {
                         ProductId = pv.ProductId,
                         VoucherId = dest.Id, // VoucherId là Id của Voucher mà bạn vừa tạo
                         Id = Guid.NewGuid() // Tạo mới GUID cho ProductVoucher
                     }).ToList();
                 }
             });

            // Ánh xạ giữa thực thể ProductVoucher và ProductVoucherDto
            CreateMap<ProductVoucher, ProductVoucherDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.VoucherId, opt => opt.MapFrom(src => src.VoucherId))
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product));

            // Ánh xạ ngược từ ProductVoucherDto sang thực thể ProductVoucher
            CreateMap<ProductVoucherDto, ProductVoucher>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ignore Id khi nhận dữ liệu từ client
                .ForMember(dest => dest.VoucherId, opt => opt.Ignore()); // Ignore VoucherId khi nhận dữ liệu từ client

            // Ánh xạ giữa thực thể ProductVoucher và CreateProductVoucherDto
            CreateMap<ProductVoucher, CreateProductVoucherDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

            // Ánh xạ ngược từ CreateProductVoucherDto sang thực thể ProductVoucher
            CreateMap<CreateProductVoucherDto, ProductVoucher>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ignore Id khi nhận dữ liệu từ client
                .ForMember(dest => dest.VoucherId, opt => opt.Ignore()); // Ignore VoucherId khi nhận dữ liệu từ client

            // Ánh xạ giữa thực thể Product và ProductDto
            CreateMap<Product, ProductDto>();
        }
    }
}
