using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.Voucher;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Voucher
{
    public interface IVoucherService : IBaseService<KhanhSkin_BackEnd.Entities.Voucher, CreateUpdateVoucherDto, CreateUpdateVoucherDto, VoucherGetRequestInputDto>
    {
    }
}
