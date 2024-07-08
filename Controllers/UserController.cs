using Microsoft.AspNetCore.Mvc;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Services.Users;
using Microsoft.Extensions.Logging; // Thêm thư viện này
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
        private readonly ILogger<UserController> _logger; // Khởi tạo logger

        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger; // Gán logger
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
                // Since ApiException is expected and used for user feedback, no need to log it.
                // Just return the error to the UI.
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log unexpected exceptions, as these are not intended for the user.
                _logger.LogError(ex, "Unexpected error occurred while creating user with email {Email}", input.Email);

                // Return a generic error message to the UI to avoid exposing sensitive error details.
                return BadRequest(new { message = "An unexpected error occurred. Please try again later." });
            }
        }



    }
}
