using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Services.Brands;
using KhanhSkin_BackEnd.Services.ProductTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.ProductType;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TypeProductController : ControllerBase
    {
        private readonly ProductTypeService _productTypeService;
        private readonly ILogger<TypeProductController> _logger;

        public TypeProductController(ProductTypeService productTypeService, ILogger<TypeProductController> logger)
        {
            _productTypeService = productTypeService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-productType")]
        public async Task<IActionResult> Create(ProductTypeDto input)
        {
            try
            {
                var result = await _productTypeService.Create(input);
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
        [HttpPut("update-productType/{id}")]
        public async Task<IActionResult> Update(Guid id, ProductTypeDto input)
        {
            try
            {
                var result = await _productTypeService.Update(id, input);
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
        [HttpDelete("delete-productType/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _productTypeService.Delete(id);
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

        [HttpGet("get-all-productTypes")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _productTypeService.GetAll();
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet("get-productType/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _productTypeService.GetById(id);
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

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string typeName)
        {
            try
            {
                var result = await _productTypeService.Search(typeName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

    }


}
