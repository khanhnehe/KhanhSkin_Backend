using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Carts
{
    public interface ICartService : IBaseService<KhanhSkin_BackEnd.Entities.Cart, CartDto, AddProductToCartDto, CartGetRequestInputDto>
    {
        Task AddProductToCart(Guid userId, Guid productId, int quantity, Guid? variantId = null);

    }
}


