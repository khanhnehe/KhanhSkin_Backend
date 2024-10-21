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


        [HttpGet("get-order-by-user-id")]
        public async Task<IActionResult> GetOrderByUserId()
        {
            try
            {
                var orderDto = await _orderService.GetOrderByUserId();
                return Ok(orderDto);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Failed to retrieve cart for the current user: {Message}", ex.Message);
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the cart for the current user: {Message}", ex.Message);
                throw new ApiException($" {ex.Message}");
            }
        }

        [HttpPost("change-status")]
        public async Task<IActionResult> ChangeStatusOrder([FromBody] ChangeStatus input)
        {
            try
            {
                var orderDto = await _orderService.ChangeStatusOrder(input);
                return Ok(orderDto);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to change order status: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while changing order status: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }

        [HttpGet("get-all-orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orderDtos = await _orderService.GetAllOrders();
                return Ok(orderDtos);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Failed to retrieve all orders: {Message}", ex.Message);
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all orders: {Message}", ex.Message);
                throw new ApiException($"{ex.Message}");
            }
        }

        [HttpPost("get-paged-orders")]
        public async Task<IActionResult> GetPagedOrders([FromBody] OrderGetRequestInputDto input)
        {
            try
            {
                var pagedOrders = await _orderService.GetPagedOrders(input);

                return Ok(pagedOrders);
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

        [HttpPost("get-orders-by-user-status")]
        public async Task<IActionResult> GetOrderByUserAndStatus([FromBody] OrderGetRequestInputDto input)
        {
            try
            {
                var orderDtos = await _orderService.GetOrderByUserIdAndStatus(input);
                return Ok(orderDtos);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to retrieve orders by status: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving orders by status: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }
    }
}
