using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Voucher;
using KhanhSkin_BackEnd.Entities;
using System.Linq;
using System.Collections.Generic;

namespace KhanhSkin_BackEnd.Services.Voucher
{
    public class VoucherAutoMapperProfile : Profile
    {
        public VoucherAutoMapperProfile()
        {
            // Mapping từ Voucher entity sang CreateUpdateVoucherDto
            CreateMap<KhanhSkin_BackEnd.Entities.Voucher, CreateUpdateVoucherDto>()
                .ForMember(dest => dest.ApplicableProductIds, opt => opt.MapFrom(src => src.ApplicableProducts.Select(p => p.Id).ToList()));

            // Mapping từ CreateUpdateVoucherDto sang Voucher entity
            CreateMap<CreateUpdateVoucherDto, KhanhSkin_BackEnd.Entities.Voucher>()
                .ForMember(dest => dest.ApplicableProducts, opt => opt.Ignore()) // Bỏ qua thông tin chi tiết của sản phẩm
                .AfterMap((dto, voucher, context) =>
                {
                    if (dto.ApplicableProductIds != null && dto.ApplicableProductIds.Any())
                    {
                        // Chuyển đổi ApplicableProductIds thành danh sách Product entity
                        voucher.ApplicableProducts = new HashSet<Product>();
                        foreach (var productId in dto.ApplicableProductIds)
                        {
                            var product = context.Mapper.Map<Product>(productId);
                            voucher.ApplicableProducts.Add(product);
                        }
                    }
                });
        }
    }
}
