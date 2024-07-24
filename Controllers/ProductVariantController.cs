using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.ProductVariant;
using KhanhSkin_BackEnd.Services.ProductVariants;
using Microsoft.AspNetCore.Mvc;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantController : ControllerBase
    {
        private readonly ProductVariantService _productVariantService;
        private readonly ILogger<ProductVariantController> _logger;

        public ProductVariantController(ProductVariantService productVariantService, ILogger<ProductVariantController> logger)
        {
            _productVariantService = productVariantService;
            _logger = logger;
        }

        [HttpPost("create-varaint")]
        public async Task<IActionResult> Create(ProductVariantDto input)
        {
            try
            {
                var result = await _productVariantService.Create(input);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to create product: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating the product: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpPut("update-variant/{id}")]
        public async Task<IActionResult> Update(Guid id, ProductVariantDto input)
        {
            try
            {
                var result = await _productVariantService.Update(id, input);
                return Ok(new ApiResponse("Cập nhật biến thể sản phẩm thành công.", result));
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to update product variant: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the product variant: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet("get-variant/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _productVariantService.Get(id);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to get product variant: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting the product variant: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet("get-all-variants")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _productVariantService.GetAll();
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to get all product variants: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting all product variants: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpDelete("delete-variant/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var variant = await _productVariantService.Delete(id);
                return Ok("Đã xóa biến thể sản phẩm thành công");
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to delete product variant with ID {id}: {ex.Message}");
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the product variant with ID {id}: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }
    }
}
