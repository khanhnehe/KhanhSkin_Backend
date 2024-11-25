using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.InventoryLog;
using KhanhSkin_BackEnd.Dtos.Voucher;
using KhanhSkin_BackEnd.Services.InventoryLogs;
using KhanhSkin_BackEnd.Services.Voucher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly VoucherService _voucherService;
        private readonly ILogger<VoucherController> _logger;

        public VoucherController(VoucherService voucherService, ILogger<VoucherController> logger)
        {
            _voucherService = voucherService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("create-voucher")]
        public async Task<IActionResult> Create([FromBody] CreateUpdateVoucherDto input)
        {
            try
            {
                var result = await _voucherService.Create(input);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to create voucher: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating the voucher: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("update-voucher/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateUpdateVoucherDto input)
        {
            try
            {
                var result = await _voucherService.Update(id, input);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to update voucher: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the voucher: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("get-all-vouchers")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var vouchers = await _voucherService.GetAll();
                return Ok(vouchers);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to retrieve all vouchers: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching all vouchers: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-voucher/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _voucherService.Delete(id);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to delete voucher: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the voucher: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("get-voucher/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var voucher = await _voucherService.Get(id);
                return Ok(voucher);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to fetch voucher with ID {id}: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching voucher with ID {id}: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("get-active-vouchers")]
        public async Task<IActionResult> GetVoucher()
        {
            try
            {
                var activeVouchers = await _voucherService.GetVoucher();
                return Ok(activeVouchers);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to retrieve active vouchers: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching active vouchers: {ex.Message}");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }


        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("get-page-voucher")]
        public async Task<IActionResult> GetVoucherPage([FromBody] VoucherGetRequestInputDto input)
        {
            try
            {
                // Gọi tới service để lấy danh sách InventoryLog phân trang dựa trên input từ body
                var pagedInventoryLogs = await _voucherService.GetVoucherPage(input);

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



