using KhanhSkin_BackEnd.Dtos.Supplier;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Services.Suppliers
{
    public interface ISupplierService : IBaseService<Supplier, SupplierDto, SupplierDto, SupplierGetRequestInputDto>
    {
        Task<PagedViewModel<SupplierDto>> GetSupplierPage(SupplierGetRequestInputDto input);
    }
}
