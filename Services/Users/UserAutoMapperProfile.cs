using AutoMapper;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Users
{
    public class UserAutoMapperProfile : Profile
    {
        public UserAutoMapperProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUpdateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Bỏ qua ánh xạ Id
                .ForMember(dest => dest.Password, opt => opt.Ignore()); // Bỏ qua ánh xạ Password

        }
    }
}
