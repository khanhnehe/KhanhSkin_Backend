﻿using System;
using System.Collections.Generic;
using KhanhSkin_BackEnd.Dtos.ProductVariant; 
using KhanhSkin_BackEnd.Dtos.Brand; 
using KhanhSkin_BackEnd.Dtos.Category; 
using KhanhSkin_BackEnd.Dtos.ProductType;
using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Product
{
    public class ProductDto : BaseDto
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int? Discount { get; set; }
        public decimal? SalePrice { get; set; }
        public string SKU { get; set; }
        public int ReviewCount { get; set; }
        public Guid BrandId { get; set; }
        public BrandDto Brand { get; set; } // Include Brand details if needed
        public int Purchases { get; set; }
        public decimal AverageRating { get; set; }
        public IList<string> Images { get; set; } = new List<string>(); // Chuyển sang IList<string>
        public ICollection<ProductVariantDto> Variants { get; set; }
        public ICollection<CategoryDto> Categories { get; set; } // Include Category details if needed
        public ICollection<ProductTypeDto> ProductTypes { get; set; } // Include ProductType details if needed
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    }
}
