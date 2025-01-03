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


        [Authorize]
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
                _logger.LogError(ex, $"{ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }


        [Authorize]
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


        [Authorize]
        [HttpGet("get-cart-by-user-id")]
        public async Task<IActionResult> GetCartByUserId()
        {
            try
            {
                var cartDto = await _cartService.GetCartByUserId();
                return Ok(cartDto);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Failed to retrieve cart for the current user: {Message}", ex.Message);
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the cart for the current user: {Message}", ex.Message);
                throw new ApiException($"{ex.Message}");
            }
        }


        [Authorize]
        [HttpPost("apply-voucher")]
        public async Task<IActionResult> ApplyVoucherToCart([FromBody] ApplyVoucherDto input)
        {
            try
            {
                await _cartService.ApplyVoucherToCart(input);
                return Ok(new { message = "Voucher applied successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                throw new ApiException($"{ex.Message}");
            }
        }

        
    }

}
