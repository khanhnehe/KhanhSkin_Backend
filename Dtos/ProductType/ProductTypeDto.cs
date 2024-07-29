using KhanhSkin_BackEnd.Share.Dtos;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.ProductType
{
    public class ProductTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Vui lòng nhập phân loại !")]
        public string TypeName { get; set; }

        // Dùng để truyền danh sách ID của các Category mà ProductType này thuộc về
    }
}
