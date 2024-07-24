using KhanhSkin_BackEnd.Share.Dtos;
using System;
using System.Collections.Generic;

namespace KhanhSkin_BackEnd.Dtos.Product
{
    public class ProductGetRequestInputDto : BaseGetRequestInput
    {
        public Guid? BrandId { get; set; } // Lọc theo BrandId
        public List<Guid> CategoryIds { get; set; } // Lọc theo danh sách CategoryIds
        public List<Guid> ProductTypeIds { get; set; } // Lọc theo danh sách ProductTypeIds

        public ProductGetRequestInputDto()
        {
            CategoryIds = new List<Guid>();
            ProductTypeIds = new List<Guid>();
        }
    }
}
