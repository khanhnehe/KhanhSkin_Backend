using Microsoft.AspNetCore.Mvc;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Services.Users;
using System;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(UserCreateDto userCreateDto)
        {
            try
            {
                var result = await _userService.Create(userCreateDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the error
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _userService.Get(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the error
                return NotFound(ex.Message);
            }
        }

        
    }
}
