using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.InventoryLog
{
    public class InventoryLogGetRequestInput: BaseGetRequestInput
    {
        public Enums.ActionType? ActionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
