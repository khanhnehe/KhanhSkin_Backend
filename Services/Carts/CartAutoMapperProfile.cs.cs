using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Dtos.CartItem;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Carts
{
    public class CartAutoMapperProfile : Profile
    {
        public CartAutoMapperProfile()
        {
            // Map từ Cart entity sang CartDto
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.CartItems, opt => opt.MapFrom(src => src.CartItems));

            // Map từ CartItem entity sang CartItemDto
            CreateMap<CartItem, CartItemDto>();
        }
    }
}
