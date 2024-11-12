using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Supplier;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Suppliers
{
    public class SupplierAutoMapperProfile : Profile
    {
        public SupplierAutoMapperProfile()
        {
            CreateMap<Supplier, SupplierDto>();
            CreateMap<SupplierDto, Supplier>()
           .ForMember(dest => dest.Id, opt => opt.Ignore());

        }
    }
}
