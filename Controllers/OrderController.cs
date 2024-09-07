using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Order;
using KhanhSkin_BackEnd.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(OrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOutOrder([FromBody] CheckoutOrderDto checkoutOrderDto)
        {
            try
            {
                var orderDto = await _orderService.CheckOutOrder(checkoutOrderDto);
                return Ok(orderDto);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to checkout order: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while checking out order: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }

        [HttpPost("apply-voucher-order")]
        public async Task<IActionResult> ApplyVoucherToOrder([FromBody] ApplyVoucherOrderDto input)
        {
            try
            {
                await _orderService.ApplyVouchertoOrder(input);
                return Ok(new { message = "Voucher applied successfully to the order." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying voucher to order.");
                return BadRequest(new { message = ex.Message });
            }
        }

        

       
    }
}
