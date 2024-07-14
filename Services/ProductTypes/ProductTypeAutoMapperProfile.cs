using AutoMapper;
using KhanhSkin_BackEnd.Dtos.ProductType;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.ProductTypes
{
    public class ProductTypeAutoMapperProfile : Profile
    {
        public ProductTypeAutoMapperProfile()
        {
            // Tạo ánh xạ từ Entity ProductType sang ProductTypeDto
            CreateMap<ProductType, ProductTypeDto>()
                // Cấu hình để ánh xạ danh sách ID của Categories từ ProductType sang ProductTypeDto
                .ForMember(dto => dto.CategoryIds, opt => opt.MapFrom(pt => pt.Categories.Select(c => c.Id)));

            // Tạo ánh xạ từ ProductTypeDto sang Entity ProductType
            CreateMap<ProductTypeDto, ProductType>()
                // Bỏ qua ánh xạ cho Id khi chuyển từ DTO sang Entity để tránh ghi đè Id tự động sinh
                .ForMember(dest => dest.Id, opt => opt.Ignore())

                // Ánh xạ trực tiếp giá trị TypeName từ DTO sang Entity
                .ForMember(entity => entity.TypeName, opt => opt.MapFrom(dto => dto.TypeName))
                // Bỏ qua ánh xạ cho Categories khi chuyển từ DTO sang Entity
                // Điều này là do chúng ta chỉ làm việc với ID của Categories thông qua DTO
                // và việc quản lý mối quan hệ giữa ProductType và Categories sẽ được xử lý riêng biệt
                .ForMember(entity => entity.Categories, opt => opt.Ignore());
        }
    }
}
