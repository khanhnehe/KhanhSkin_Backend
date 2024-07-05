﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class Product : BaseEntity
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm!")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm!")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm!")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0!")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng sản phẩm!")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn hoặc bằng 0!")]
        public int Quantity { get; set; }

        [Range(0, 100, ErrorMessage = "Giảm giá phải từ 0 đến 100!")]
        public int? Discount { get; set; }

        public decimal? SalePrice { get; set; }

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        [Required(ErrorMessage = "Vui lòng nhập SKU của sản phẩm!")]
        public string SKU { get; set; }

        // Foreign keys
        public Guid BrandId { get; set; }
        [ForeignKey("BrandId")]
        public Brand Brand { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<ProductType> ProductTypes { get; set; } = new List<ProductType>();

        public int Purchases { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public decimal AverageRating { get; set; }

        public ICollection<string> Images { get; set; } = new List<string>();
    }
}