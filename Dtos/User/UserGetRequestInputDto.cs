using KhanhSkin_BackEnd.Share.Dtos;
using static KhanhSkin_BackEnd.Consts.Enums;

namespace KhanhSkin_BackEnd.Dtos.User
{
    public class UserGetRequestInputDto : BaseGetRequestInput
    {
        public Role? Role { get; set; }

    }
}
