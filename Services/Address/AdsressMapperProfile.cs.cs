using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Address;
using KhanhSkin_BackEnd.Entities;

using Addresses = KhanhSkin_BackEnd.Entities.Address;

namespace KhanhSkin_BackEnd.Services.Address
{
    public class AddressMapperProfile : Profile
    {
        public AddressMapperProfile()
        {
            // Cấu hình ánh xạ từ Address entity sang AddressDto
            CreateMap<Addresses, AddressDto>();

            // Cấu hình ánh xạ từ CreateUpdateAddressDto sang Address entity và ngược lại
            CreateMap<CreateUpdateAddressDto, Addresses>().ReverseMap();

            // Nếu có các thuộc tính cần ánh xạ đặc biệt, bạn có thể cấu hình chúng ở đây
            // Ví dụ:
            // CreateMap<Address, AddressDto>()
            //     .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src => $"{src.AddressDetail}, {src.Ward}, {src.District}, {src.Province}"));
        }
    }
}

