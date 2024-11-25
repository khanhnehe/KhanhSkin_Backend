using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Review;
using KhanhSkin_BackEnd.Services.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(ReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }


        [Authorize]
        [HttpPost("create-reviews")]
        public async Task<IActionResult> CreateReviews([FromBody] List<CreateReviewDto> inputs)
        {
            try
            {
                // Gọi phương thức CreateReviews trong ReviewService
                var reviews = await _reviewService.CreateReviews(inputs);
                return Ok(reviews);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to create multiple reviews: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating multiple reviews: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }


        [Authorize]
        [HttpPost("get-paged-reviews")]
            public async Task<IActionResult> GetPagedReviews([FromBody] ReviewGetRequestInputDto input)
            {
                try
                {
                    // Gọi tới service để lấy đánh giá phân trang dựa trên input từ body
                    var pagedReviews = await _reviewService.GetPagedReviews(input);

                    // Trả về kết quả với dữ liệu đánh giá đã phân trang
                    return Ok(pagedReviews);
                }
                catch (ApiException ex)
                {
                    _logger.LogError(ex, $"Failed to fetch paged reviews: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while fetching paged reviews: {ex.Message}");
                throw new ApiException($" {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("get-review-product")]
        public async Task<IActionResult> GetReviewProduct([FromBody] ReviewGetRequestInputDto input)
        {
            try
            {
                // Gọi tới service để lấy đánh giá phân trang dựa trên input từ body
                var pagedReviews = await _reviewService.GetReviewProduct(input);

                // Trả về kết quả với dữ liệu đánh giá đã phân trang
                return Ok(pagedReviews);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Failed to fetch paged reviews: {ex.Message}");
                throw new ApiException(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching paged reviews: {ex.Message}");
                throw new ApiException($" {ex.Message}");
            }
        }


        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("change-approval")]
        public async Task<IActionResult> ChangeApprovalStatus([FromBody] ChangeStatusReview input)
        {
            try
            {
                var updatedReview = await _reviewService.ChangeApprovalStatus(input);
                return Ok(updatedReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while changing the review approval status: {ex.Message}");
                throw new ApiException($"{ex.Message}");
            }
        }

    }
}
