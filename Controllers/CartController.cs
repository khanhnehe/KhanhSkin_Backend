﻿using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Dtos.CartItem;
using KhanhSkin_BackEnd.Services.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(CartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpPost("add-product-to-cart")]
        public async Task<IActionResult> AddProductToCart(AddProductToCartDto input)
        {
            try
            {
                var cartDto = await _cartService.AddProductToCart(input);
                return Ok(cartDto);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to add product to cart: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while adding product to cart: {ex.Message}");
                throw new ApiException($"An error occurred: {ex.Message}");
            }
        }
        [HttpDelete("delete/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(Guid cartItemId)
        {
            try
            {
                var cartDto = await _cartService.RemoveCartItem(cartItemId);
                return Ok(new { message = "Cart item removed successfully.", result = cartDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing item from cart.");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-cart-by-user-id/{userId}")]
        public async Task<IActionResult> GetCartByUserId(Guid userId)
        {
            try
            {
                var cartDto = await _cartService.GetCartByUserId(userId);
                return Ok(cartDto);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to retrieve cart for user ID {userId}: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the cart for user ID {userId}: {ex.Message}");
                throw new ApiException($"An error occurred: {ex.Message}");
            }
        }

        // Other methods in CartController...
    }
}