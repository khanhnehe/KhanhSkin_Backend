using KhanhSkin_BackEnd.Services.Address;
using Microsoft.AspNetCore.Mvc;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Address;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly AddressService _addressService;
        private readonly ILogger<AddressController> _logger;

        public AddressController(AddressService addressService, ILogger<AddressController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("create-address")]
        public async Task<IActionResult> Create([FromBody] CreateUpdateAddressDto input)
        {
            try
            {
                var result = await _addressService.Create(input);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating address.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPut("update-address/{addressId}")]
        public async Task<IActionResult> Update(Guid addressId, [FromBody] CreateUpdateAddressDto input)
        {
            try
            {
                input.Id = addressId; // Ensure the DTO has the correct addressId
                var result = await _addressService.Update(addressId, input);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to update address with ID {addressId}: {ex.Message}");
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while updating address with ID {addressId}.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("delete-address/{addressId}")]
        public async Task<IActionResult> Delete(Guid addressId)
        {
            try
            {
                var result = await _addressService.Delete(addressId);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to delete address with ID {addressId}: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while deleting address with ID {addressId}.");
                throw new ApiException($" {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("get-all-addresses")]
        public async Task<IActionResult> GetAllAddresses()
        {
            try
            {
                var result = await _addressService.GetAllAddressesAsync();
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all addresses.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching all addresses.");
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("get-my-address")]
        public async Task<IActionResult> GetMyAddress()
        {
            try
            {
                var result = await _addressService.GetAllAddressesByUserId();
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound("No address found for the current user.");
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Failed to fetch address for the current user.");
                return StatusCode(500, $"Failed to fetch address: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching the address for the current user.");
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
