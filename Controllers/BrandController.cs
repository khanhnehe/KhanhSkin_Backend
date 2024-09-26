using KhanhSkin_BackEnd.Services.Brands;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Dtos.Brand;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly BrandService _brandService;
        private readonly ILogger<BrandController> _logger;

        public BrandController(BrandService brandService, ILogger<BrandController> logger)
        {
            _brandService = brandService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-brand")]
        public async Task<IActionResult> Create([FromForm] BrandDto input)
        {
            try
            {
                var result = await _brandService.Create(input);
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

        [Authorize(Roles = "Admin")]
        [HttpPut("Update-brand/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] BrandDto input)
        {
            try
            {
                var result = await _brandService.Update(id, input);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating brand: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-brand/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var brand = await _brandService.Delete(id);
                if (brand != null)
                {
                    return Ok("Đã xóa thương hiệu thành công"); // Trả về đối tượng Brand đã xóa
                }
                return NotFound("Không tìm thấy thương hiệu để xóa.");
            }
            catch (ApiException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("get-all-brand")]

        public async Task<IActionResult> GetAll()
        {
            try
            {
                var results = await _brandService.GetAll();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all brands: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("get-brand-by-id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _brandService.GetById(id);
                if (result != null)
                {
                    return Ok(result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting brand by id: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search-brand")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            try
            {
                var results = await _brandService.Search(keyword);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching brands: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
