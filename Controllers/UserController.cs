using Microsoft.AspNetCore.Mvc;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Services.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


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

        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        public async Task<IActionResult> Create(CreateUpdateUserDto input)
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

        [Authorize]
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

        [Authorize]
        [HttpGet("get-user-by-id/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                return Ok(user);
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

        [Authorize]
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                return Ok(users);
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

        [Authorize]
        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] CreateUpdateUserDto input)
        {
            try
            {
                var updatedUser = await _userService.UpdateUser(id, input);
                return Ok(updatedUser);
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


        [HttpDelete("delete-user/{id}")]
        [Authorize] 
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUser(id);
                if (result)
                {
                    return Ok(new { message = "Người dùng đã được xóa thành công." });
                }
                else
                {
                    return NotFound(new { message = "Không tìm thấy người dùng để xóa." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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

        [Authorize]
        [HttpGet("filter-users")]
        public async Task<IActionResult> CreateFilteredQuery([FromQuery] UserGetRequestInputDto input)
        {
            try
            {
                //_userService.CreateFilteredQuery(input) trả về IQueryable<User>
                var query = _userService.CreateFilteredQuery(input);

                // Chuyển đổi IQueryable thành List bằng cách sử dụng ToListAsync() để có thể await
                var users = await query.ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng đã lọc.");
                return StatusCode(500, "Có lỗi xảy ra khi lấy danh sách người dùng đã lọc.");
            }
        }


    }
}
