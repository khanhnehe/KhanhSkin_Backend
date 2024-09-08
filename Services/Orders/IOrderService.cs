using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Dtos.Order;

namespace KhanhSkin_BackEnd.Services.Orders
{
    public interface IOrderService : IBaseService<Order, OrderDto, OrderItemDto, OrderGetRequestInputDto>
    {
        Task<OrderDto> CheckOutOrder(CheckoutOrderDto input);
        Task<OrderDto> GetOrderByUserId(Guid userId);

    }
}
