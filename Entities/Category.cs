using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Entities
{
    public class Category : BaseEntity
    {
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục!")]
        public string CategoryName { get; set; }

        // Khởi tạo ICollection để tránh lỗi null reference
        public ICollection<ProductType> ProductTypes { get; set; } = new List<ProductType>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
