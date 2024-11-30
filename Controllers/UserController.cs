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
        [Consumes("multipart/form-data")]
        [HttpPost("create-user")]
        public async Task<IActionResult> Create([FromForm] CreateUpdateUserDto input)
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
                await _userService.ChangePassword( input.OldPassword, input.NewPassword);
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
        [HttpGet("get-user-by-id")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Gọi phương thức GetUserById từ _userService mà không cần truyền ID
                var user = await _userService.GetUserById();
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

        [Authorize(Roles = "Admin")]
        //[Authorize]
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

        [Authorize(Roles = "Admin")]
        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromForm] CreateUpdateUserDto input)
        {
            try
            {
                var updatedUser = await _userService.Update(id, input);
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

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _userService.Delete(id);
                if (user !=null)
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

        [Authorize(Roles = "Admin,Staff")]
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
