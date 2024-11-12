using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Supplier;
using KhanhSkin_BackEnd.Services.Suppliers;
using Microsoft.AspNetCore.Mvc;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly SupplierService _supplierService;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(SupplierService supplierService, ILogger<SupplierController> logger)
        {
            _supplierService = supplierService;
            _logger = logger;
        }

        [HttpPost("create-supplier")]
        public async Task<IActionResult> CreateSupplier([FromBody] SupplierDto input)
        {
            try
            {
                // Gọi phương thức Create trong SupplierService
                var supplier = await _supplierService.Create(input);
                return Ok(supplier);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Không thể tạo nhà cung cấp: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Đã xảy ra lỗi khi tạo nhà cung cấp: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }

        [HttpPut("update-supplier/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SupplierDto input)
        {
            try
            {
                var updatedSupplier = await _supplierService.Update(id, input);
                return Ok(updatedSupplier);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Không thể cập nhật nhà cung cấp với ID {id}: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Đã xảy ra lỗi khi cập nhật nhà cung cấp với ID {id}: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }

        [HttpDelete("delete-supplier/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _supplierService.Delete(id);
                return Ok("Nhà cung cấp đã được xóa thành công");
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Không thể xóa nhà cung cấp với ID {id}: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Đã xảy ra lỗi khi xóa nhà cung cấp với ID {id}: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }

        [HttpPost("get-paged-supplier")]
        public async Task<IActionResult> GetSupplierPage([FromBody] SupplierGetRequestInputDto input)
        {
            try
            {
                var pagedSupplier = await _supplierService.GetSupplierPage(input);
                return Ok(pagedSupplier);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Không thể lấy danh sách nhà cung cấp phân trang: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Đã xảy ra lỗi không mong muốn khi lấy danh sách nhà cung cấp phân trang: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }
    }
}
