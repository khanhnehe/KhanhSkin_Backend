using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using KhanhSkin_BackEnd.Consts;

namespace KhanhSkin_BackEnd.Services.Users
{
    public interface IUserService : IBaseService<User, UserDto, CreateUpdateUserDto, UserGetRequestInputDto>
    {

        Task<bool> ChangePassword(string currentPassword, string newPassword);
        Task<User> CreateUser(CreateUpdateUserDto input);



        Task<List<UserDto>> GetUsersByRole(Enums.Role role);

        Task<string> SignIn(SignInDto input);

        // Thêm các phương thức mới
        Task<UserDto> GetUserById(Guid id);
        Task<List<UserDto>> GetAllUsers();

    }

}
