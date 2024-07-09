// UserController.cs
using Microsoft.AspNetCore.Mvc;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Services.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(UserCreateDto input)
        {
            try
            {
                var result = await _userService.Create(input);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                throw new ApiException($"{ex.Message}");
            }
            catch (Exception ex)
            {
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto input)
        {
            try
            {
                await _userService.ChangePassword(input.Email, input.OldPassword, input.NewPassword);
                return Ok(new { message = "Mật khẩu đã được cập nhật thành công." });
            }
            catch (ApiException ex)
            {
                throw new ApiException($"{ex.Message}");
            }
            catch (Exception ex)
            {
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn(SignInDto input)
        {
            try
            {
                var token = await _userService.SignIn(input);
                return Ok(new { token });
            }
            catch (ApiException ex)
            {
                throw new ApiException($"{ex.Message}");
            }
            catch (Exception ex)
            {
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }
    }
}
