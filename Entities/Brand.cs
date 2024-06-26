using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Entities
{
    public class Brand : BaseEntity
    {
        [Required(ErrorMessage = "Vui lòng nhập tên thương hiệu!")]
        public string BrandName { get; set; }

        // Khởi tạo ICollection để tránh lỗi null reference
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
