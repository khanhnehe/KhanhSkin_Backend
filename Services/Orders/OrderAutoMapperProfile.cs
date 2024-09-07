using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Order;
using KhanhSkin_BackEnd.Dtos.Address;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Orders
{
    public class OrderMapperProfile : Profile
    {
        public OrderMapperProfile()
        {
            // Mapping từ Order sang OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address)) // Ánh xạ Address
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems)); // Ánh xạ OrderItems

            // Mapping từ Cart sang Order
            CreateMap<Cart, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.CartItems))
                .ForMember(dest => dest.DiscountValue, opt => opt.MapFrom(src => src.DiscountValue))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.FinalPrice, opt => opt.MapFrom(src => src.FinalPrice));

            // Mapping từ CartItem sang OrderItem
            CreateMap<CartItem, OrderItem>()
                .ForMember(dest => dest.CartItemId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.VariantId))
                .ForMember(dest => dest.NameVariant, opt => opt.MapFrom(src => src.NameVariant))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.ProductPrice))
                .ForMember(dest => dest.ProductSalePrice, opt => opt.MapFrom(src => src.ProductSalePrice))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.ItemsPrice, opt => opt.MapFrom(src => src.ItemsPrice))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

            // Mapping từ OrderItem sang OrderItemDto
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.ProductPrice))
                .ForMember(dest => dest.ProductSalePrice, opt => opt.MapFrom(src => src.ProductSalePrice))
                .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.VariantId))
                .ForMember(dest => dest.NameVariant, opt => opt.MapFrom(src => src.NameVariant))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));


            // Mapping từ Voucher sang VoucherSnapshot
            CreateMap<Entities.Voucher, string>()
                .ConvertUsing(src => src.Code);
        }
    }
}
