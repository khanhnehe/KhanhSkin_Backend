using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.InventoryLog;
using KhanhSkin_BackEnd.Services.InventoryLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryLogService _inventoryLogService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(InventoryLogService inventoryLogService, ILogger<InventoryController> logger)
        {
            _inventoryLogService = inventoryLogService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("get-paged-logs")]
        public async Task<IActionResult> GetPagedInventoryLogs([FromBody] InventoryLogGetRequestInput input)
        {
            try
            {
                // Gọi tới service để lấy danh sách InventoryLog phân trang dựa trên input từ body
                var pagedInventoryLogs = await _inventoryLogService.GetPagedInventoryLogs(input);

                // Trả về kết quả với dữ liệu InventoryLog đã phân trang
                return Ok(pagedInventoryLogs);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to fetch paged inventory logs: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching paged inventory logs: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }

    }
}
