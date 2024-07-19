using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Services.Products;
using Microsoft.AspNetCore.Mvc;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpPost("create-product")]
        public async Task<IActionResult> Create(CreateUpdateProductDto input)
        {
            try
            {
                var result = await _productService.Create(input);
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

        [HttpGet("get-all-product")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productService.GetAll();
                return Ok(products);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching all products.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet("get-by-Id-product/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var product = await _productService.Get(id);
                return Ok(product);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to fetch product with ID {id}: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching product with ID {id}.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpPut("update-product/{productId}")]
        public async Task<IActionResult> Update(Guid productId, [FromBody] CreateUpdateProductDto input)
        {
            try
            {
                var updatedProduct = await _productService.Update(productId, input);
                return Ok(updatedProduct);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to update product with ID {productId}: {ex.Message}");
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the product with ID {productId}: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }
    }
}
