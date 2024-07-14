using KhanhSkin_BackEnd.Dtos.ProductType;
using KhanhSkin_BackEnd.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Services.ProductTypes
{
    public interface IProductTypeService : IBaseService<ProductType, ProductTypeDto, ProductTypeDto, ProductTypeGetRequestInputDto>
    {
        // Thêm phương thức Search vào interface
        Task<List<ProductTypeDto>> Search(string typeName);
    }
}
