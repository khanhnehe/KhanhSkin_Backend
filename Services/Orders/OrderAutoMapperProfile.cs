using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Order;
using KhanhSkin_BackEnd.Dtos.Address;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Helper;
using KhanhSkin_BackEnd.Dtos.User;

namespace KhanhSkin_BackEnd.Services.Orders
{
    public class OrderMapperProfile : Profile
    {
        public OrderMapperProfile()
        {
            // Mapping từ Order sang OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => new UserDto { FullName = src.User.FullName })) // Chỉ ánh xạ FullName
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems)) // Ánh xạ OrderItems
                .ForMember(dest => dest.ShippingMethodDes, opt => opt.MapFrom(src => src.ShippingMethod.GetDescription())) // Ánh xạ mô tả ShippingMethod
                .ForMember(dest => dest.PaymentMethodDes, opt => opt.MapFrom(src => src.PaymentMethod.GetDescription())) // Ánh xạ mô tả PaymentMethod
                .ForMember(dest => dest.OrderStatusDes, opt => opt.MapFrom(src => src.OrderStatus.GetDescription())); // Ánh xạ mô tả OrderStatus

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
