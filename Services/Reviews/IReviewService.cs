using KhanhSkin_BackEnd.Dtos.Review;
using KhanhSkin_BackEnd.Entities;

namespace KhanhSkin_BackEnd.Services.Reviews
{
    public interface IReviewService : IBaseService<Review, ReviewDto, CreateReviewDto, ReviewGetRequestInputDto>
    {
        Task<List<ReviewDto>> CreateReviews(List<CreateReviewDto> input);
    }
}
