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

        // Thiết lập khóa ngoại và quan hệ với Category
        public Guid CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        // Khởi tạo ICollection để tránh lỗi null reference
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
