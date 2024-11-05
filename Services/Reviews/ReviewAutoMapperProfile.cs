using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Review;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.ProductVariant;

namespace KhanhSkin_BackEnd.Services.Reviews
{
    public class ReviewAutoMapperProfile : Profile
    {
        public ReviewAutoMapperProfile()
        {
            CreateMap<Review, ReviewDto>();
               
        }
    }
}
