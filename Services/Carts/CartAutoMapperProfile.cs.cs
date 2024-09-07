using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Dtos.CartItem;
using KhanhSkin_BackEnd.Dtos.Voucher;
using KhanhSkin_BackEnd.Dtos.Order; // Import Order DTOs
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Carts
{
    public class CartAutoMapperProfile : Profile
    {
        public CartAutoMapperProfile()
        {
            // Ánh xạ từ Cart entity sang CartDto
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.CartItems, opt => opt.MapFrom(src => src.CartItems));

            // Ánh xạ từ CartItem entity sang CartItemDto
            CreateMap<CartItem, CartItemDto>();

            // Ánh xạ từ Voucher entity sang VoucherDto
            CreateMap<KhanhSkin_BackEnd.Entities.Voucher, VoucherDto>();

           
        }
    }
}
