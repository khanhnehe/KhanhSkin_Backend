using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Dtos.Category;
using KhanhSkin_BackEnd.Services.Brands;
using KhanhSkin_BackEnd.Services.Categories;
using Microsoft.AspNetCore.Mvc;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {

        private readonly CategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(CategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpPost("create-category")] // Add this line
        public async Task<IActionResult> Create(CategoryDto input)
        {
            try
            {
                var result = await _categoryService.Create(input);
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

        [HttpPut("update/{categoryId}/product-types")]
        public async Task<IActionResult> UpdateCategoryProductTypes(Guid categoryId, [FromBody] List<Guid> productTypeIds)
        {
            try
            {
                var updatedCategoryType = await _categoryService.UpdateCategoryProductTypes(categoryId, productTypeIds);
                return Ok( updatedCategoryType);
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


        //[HttpPut("Modify/{categoryId}/product-types")]
        //public async Task<IActionResult> ModifyCategoryProductTypes(Guid categoryId, [FromBody] List<Guid> productTypeIds)
        //{
        //    try
        //    {
        //        var updatedCategory = await _categoryService.ModifyCategoryProductType(categoryId, productTypeIds);
        //        return Ok(updatedCategory);
        //    }
        //    catch (ApiException ex)
        //    {
        //        return StatusCode(ex.StatusCode, ex.Message); // Use StatusCode to return the specific HTTP status code from ApiException
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
        //    }
        //}


        //[HttpPost("add/{categoryId}/product-types")]
        //public async Task<IActionResult> AddProductTypes(Guid categoryId, [FromBody] List<Guid> productTypeIds)
        //{
        //    try
        //    {
        //        var result = await _categoryService.AddProductTypes(categoryId, productTypeIds);
        //        return Ok(result);
        //    }
        //    catch (ApiException ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An unexpected error occurred.");
        //        throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
        //    }
        //}

        [HttpGet("get-all-category")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetAll();
                return Ok(categories);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching all categories.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet("get-by-Id-category/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var category = await _categoryService.Get(id);
                return Ok(category);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to fetch category with ID {id}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching category with ID {id}.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpPut("update-category/{categoryId}")] // Use the HTTP PUT method for updates, and capture the categoryId from the route
        public async Task<IActionResult> Update(Guid categoryId, [FromBody] CategoryDto input)
        {
            try
            {
                // Ensure the input DTO has the correct categoryId, especially if the DTO includes the ID as a field
                input.Id = categoryId;

                var updatedCategory = await _categoryService.Update(categoryId, input);
                return Ok(updatedCategory);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to update category with ID {categoryId}: {ex.Message}");
                return StatusCode(ex.StatusCode, ex.Message); // Use StatusCode to return the specific HTTP status code from ApiException
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while updating category with ID {categoryId}.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        //[HttpPut("update/{categoryId}/product-types")]
        //public async Task<IActionResult> UpdateCategoryProductTypes(Guid categoryId, [FromBody] List<Guid> productTypeIds)
        //{
        //    try
        //    {
        //        var result = await _categoryService.UpdateCategoryProductType(categoryId, productTypeIds);
        //        return Ok(result);
        //    }
        //    catch (ApiException ex)
        //    {
        //        _logger.LogError(ex, $"Failed to update product types for category with ID {categoryId}: {ex.Message}");
        //        return StatusCode(ex.StatusCode, ex.Message); // Sử dụng StatusCode để trả về mã trạng thái HTTP cụ thể từ ApiException
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"An unexpected error occurred while updating product types for category with ID {categoryId}.");
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}"); // Trả về lỗi 500 Internal Server Error cho các ngoại lệ không mong đợi
        //    }
        //}

        //[HttpPost("{categoryId}/product-types")]
        //public async Task<IActionResult> UpdateCategoryWithProductTypes(Guid categoryId, [FromBody] List<Guid> productTypeIds)
        //{
        //    try
        //    {
        //        var result = await _categoryService.UpdateCategoryProductType(categoryId, productTypeIds);
        //        return Ok(result);
        //    }
        //    catch (ApiException ex)
        //    {
        //        throw new ApiException($"{ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
        //    }
        //}

    }
}
