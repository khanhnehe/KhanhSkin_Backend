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
            CreateMap<UserCreateDto, User>();
        }
    }
}
