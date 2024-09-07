using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Entities;
using System;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Services.Carts
{
    public interface ICartService : IBaseService<Cart, CartDto, AddProductToCartDto, CartGetRequestInputDto>
    {
        Task<CartDto> AddProductToCart(AddProductToCartDto input);
        Task ApplyVoucherToCart(ApplyVoucherDto input);

        Task<CartDto> GetCartByUserId(Guid userId);
        Task<CartDto> RemoveCartItem(Guid cartItemId);
    }
}
