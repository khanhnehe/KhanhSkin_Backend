using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Brands
{
    public class BrandAutoMapperProfile : Profile
    {
        public BrandAutoMapperProfile()
        {
            CreateMap<Brand, BrandDto>();

            // Thêm cấu hình ánh xạ từ DTO sang Entity
            // Trong cấu hình AutoMapper của bạn
            CreateMap<BrandDto, Brand>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // Loại trừ Id khỏi quá trình ánh xạ
        }
    }
}
