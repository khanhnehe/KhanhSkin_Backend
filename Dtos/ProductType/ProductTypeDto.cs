using KhanhSkin_BackEnd.Share.Dtos;
using System.Collections.Generic;

namespace KhanhSkin_BackEnd.Dtos.ProductType
{
    public class ProductTypeDto : BaseDto
    {
        public string TypeName { get; set; }

        // Dùng để truyền danh sách ID của các Category mà ProductType này thuộc về
    }
}
