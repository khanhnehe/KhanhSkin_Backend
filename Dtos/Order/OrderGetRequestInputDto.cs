using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Order
{
    public class OrderGetRequestInputDto : BaseGetRequestInput
    {
        public Enums.OrderStatus? OrderStatus { get; set; }

    }
}
