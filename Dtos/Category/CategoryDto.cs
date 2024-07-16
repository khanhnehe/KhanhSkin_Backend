using KhanhSkin_BackEnd.Dtos.ProductType;
using KhanhSkin_BackEnd.Share.Dtos;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.Category
{
    public class CategoryDto : BaseDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục!")]
        public string CategoryName { get; set; }

        // Danh sách ID của ProductType liên quan đến Category này
        public List<Guid> ProductTypeIds { get; set; } = new List<Guid>();

        // Danh sách ID của Product liên quan đến Category này
        public List<Guid> ProductIds { get; set; } = new List<Guid>();

        // Thêm thuộc tính này
        public List<ProductTypeDto> ProductTypes { get; set; } = new List<ProductTypeDto>();
    }
}
