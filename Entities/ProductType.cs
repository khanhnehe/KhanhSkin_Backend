using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class ProductType : BaseEntity
    {
            [Required(ErrorMessage = "Vui lòng nhập tên loại sản phẩm!")]
            public string TypeName { get; set; }

            // Khởi tạo ICollection để tránh lỗi null reference
            public ICollection<Product> Products { get; set; } = new List<Product>();

            // Thêm ICollection<Category> để thể hiện mối quan hệ nhiều-nhiều
            public ICollection<Category> Categories { get; set; } = new List<Category>();
        }
}
