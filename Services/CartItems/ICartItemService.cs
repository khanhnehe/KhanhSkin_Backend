using System;
using System.Threading.Tasks;
using KhanhSkin_BackEnd.Dtos.CartItem;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.CartItems
{
    public interface ICartItemService : IBaseService<CartItem, CartItemDto, CartItemDto, CartItemGetRequestInput>
    {
    }
}
