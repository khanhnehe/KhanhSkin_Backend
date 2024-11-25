using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly ProductRecommenService _productRecommenService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ProductService productService,
                  ProductRecommenService productRecommenService,
                     ILogger<ProductController> logger)
        {
            _productService = productService;
            _productRecommenService = productRecommenService;

            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-product")]
        public async Task<IActionResult> Create([FromBody]  CreateUpdateProductDto input)
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


        [Authorize]
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


        [Authorize(Roles = "Admin")]
        [HttpPut("update-product/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] CreateUpdateProductDto input)
        {
            try
            {
                var updatedProduct = await _productService.Update(id, input);
                return Ok(updatedProduct);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to update produc: {ex.Message}");
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the product: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-product/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var product = await _productService.Delete(id);
                return Ok("Đã xóa sản phẩm thành công");
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to delete product with ID {id}: {ex.Message}");
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the product with ID {id}: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }



        [HttpPost("post-filte-products")]
        public async Task<IActionResult> PostFilteredProducts([FromBody] ProductGetRequestInputDto input)
        {
            try
            {
                // Sử dụng phương thức CreateFilteredQuery từ service để lọc sản phẩm
                var products = await _productService.GetFilteredProducts(input);
                return Ok(products); // Trả về danh sách sản phẩm đã lọc
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to filter products: {ex.Message}");
                return StatusCode(ex.StatusCode, new { error = ex.Message }); // Trả về lỗi tùy chỉnh
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while filtering products: {ex.Message}");
                return StatusCode(500, new { error = $"Có lỗi xảy ra: {ex.Message}" }); // Trả về lỗi chung
            }
        }




        [HttpGet("search-product")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            try
            {
                var results = await _productService.Search(keyword);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching brands: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("get-by-category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(Guid categoryId)
        {
            try
            {
                var products = await _productService.GetByCategory(categoryId);
                return Ok(products);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to fetch products by category ID {categoryId}: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching products by category ID {categoryId}.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet("get-by-producttype/{productTypeId}")]
        public async Task<IActionResult> GetByProductType(Guid productTypeId)
        {
            try
            {
                var products = await _productService.GetByProductType(productTypeId);
                return Ok(products);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to fetch products by product type ID {productTypeId}: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching products by product type ID {productTypeId}.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet("get-by-brand/{brandId}")]
        public async Task<IActionResult> GetByBrand(Guid brandId)
        {
            try
            {
                var products = await _productService.GetByBrand(brandId);
                return Ok(products);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to fetch products by brand ID {brandId}: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching products by brand ID {brandId}.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }


        [HttpPost("get-paged-products")]
        public async Task<IActionResult> GetPagedProducts([FromBody] ProductGetRequestInputDto input)
        {
            try
            {
                // Gọi tới service để lấy sản phẩm phân trang dựa trên input từ body
                var pagedProducts = await _productService.GetPagedProducts(input);

                // Trả về kết quả với dữ liệu sản phẩm đã phân trang
                return Ok(pagedProducts);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to fetch paged products: {ex.Message}");
                return StatusCode(ex.StatusCode, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching paged products: {ex.Message}");
                return StatusCode(500, new { error = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("import-inventory")]
        public async Task<IActionResult> ImportInventory([FromBody] List<ProductInventoryImportDto> inputs) // Đổi kiểu thành List nếu cần
        {
            try
            {
                await _productService.ImportProductInventoryAsync(inputs);
                return Ok(new ApiResponse("Nhập hàng thành công."));
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to import inventory: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while importing inventory: {ex.Message}");
                throw new ApiException($" {ex.Message}");
            }
        }



        // Endpoint để lấy danh sách sản phẩm gợi ý
        [HttpGet("{productId}/recommendations")]
        public async Task<IActionResult> RecommendProducts(string productId, int topN = 5)
        {
            try
            {
                // Gọi ProductRecommenService để lấy gợi ý
                var recommendedProducts = await _productService.RecommendProductsAsync(productId, topN);

                if (recommendedProducts == null)
                {
                    return NotFound(new ApiResponse(404, "Không tìm thấy gợi ý cho sản phẩm này."));
                }

                return Ok(new ApiResponse(recommendedProducts));
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to fetch recommendations for Product ID: {productId}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching recommendations for Product ID: {productId}: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

    }
}
