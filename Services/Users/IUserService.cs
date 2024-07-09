using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using KhanhSkin_BackEnd.Consts;

namespace KhanhSkin_BackEnd.Services.Users
{
    public interface IUserService : IBaseService<User, UserDto, UserCreateDto, UserGetRequestInputDto>
    {
       
        Task<bool> ChangePassword(string Email, string Password);
       
        Task<List<UserDto>> GetUsersByRole(Enums.Role role);

        Task<string> SignIn(SignInDto input);

    }

}
