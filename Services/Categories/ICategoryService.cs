using System;
using System.Threading.Tasks;
using KhanhSkin_BackEnd.Dtos.Category;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Categories
{
    public interface ICategoryService : IBaseService<Category, CategoryDto, CategoryDto, CategoryGetRequestInputDto>
    {
        Task<CategoryDto> AddProductType(Guid categoryId, Guid productTypeId);
        // Bạn có thể thêm các phương thức khác tại đây nếu cần
    }
}
