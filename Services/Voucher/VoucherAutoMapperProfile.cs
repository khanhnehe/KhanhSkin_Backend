using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Voucher;
using KhanhSkin_BackEnd.Entities;

public class VoucherAutoMapperProfile : Profile
{
    public VoucherAutoMapperProfile()
    {
        // Mapping từ Voucher entity sang CreateUpdateVoucherDto
        CreateMap<Voucher, CreateUpdateVoucherDto>()
            .ForMember(dest => dest.ApplicableProductIds, opt => opt.MapFrom(src => src.ApplicableProducts.Select(p => p.Id).ToList()));

        // Mapping từ CreateUpdateVoucherDto sang Voucher entity, bỏ qua ApplicableProducts
        CreateMap<CreateUpdateVoucherDto, Voucher>()
            .ForMember(dest => dest.ApplicableProducts, opt => opt.Ignore());
    }
}
