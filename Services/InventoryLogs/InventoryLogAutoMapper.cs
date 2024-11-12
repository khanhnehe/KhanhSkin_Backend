using AutoMapper;
using KhanhSkin_BackEnd.Dtos.InventoryLog;

namespace KhanhSkin_BackEnd.Services.InventoryLog
{
    public class InventoryLogAutoMapper: Profile
    {
        public InventoryLogAutoMapper()
        {
            CreateMap<KhanhSkin_BackEnd.Entities.InventoryLog, InventoryLogDto>();
            CreateMap<InventoryLogDto, KhanhSkin_BackEnd.Entities.InventoryLog>();
        }
    }
}
