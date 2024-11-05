using AutoMapper;
using CloudinaryDotNet.Actions;
using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Controllers;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.ProductVariant;
using KhanhSkin_BackEnd.Dtos.Review;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Services.Orders;
using KhanhSkin_BackEnd.Services.Products;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.EntityFrameworkCore;

namespace KhanhSkin_BackEnd.Services.Reviews
{
    public class ReviewService : BaseService<Review, ReviewDto, CreateReviewDto, ReviewGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly ProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;
        private readonly ICurrentUser _currentUser;

        public ReviewService(
           IConfiguration config,
           IRepository<Review> reviewRepository,
           IRepository<Order> orderRepository,
           IRepository<Product> productRepository,
           ProductService productService,
           IMapper mapper,
           ILogger<ReviewService> logger,
           ICurrentUser currentUser)
           : base(mapper, reviewRepository, logger, currentUser)
        {
            _config = config;
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;

        }

        public async Task<List<ReviewDto>> CreateReviews(List<CreateReviewDto> input)
        {
            var userId = _currentUser.Id;
            if (userId == null)
            {
                throw new Exception("User not authenticated");
            }

            var reviewDtos = new List<ReviewDto>();
            Order? reviewedOrder = null;

            foreach (var reviewInput in input)
            {
                // Kiểm tra và log nếu `order` là `null`
                var order = await _orderRepository
                    .AsQueryable()
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == reviewInput.OrderId && o.UserId == userId && o.OrderStatus == Enums.OrderStatus.Completed);

                if (order == null)
                {
                    _logger.LogWarning($"No completed order found for Order ID: {reviewInput.OrderId}. Skipping this review.");
                    continue;
                }

                // Kiểm tra và log nếu `orderItem` là `null`
                var orderItem = order.OrderItems.FirstOrDefault(item => item.ProductId == reviewInput.ProductId);
                if (orderItem == null)
                {
                    _logger.LogWarning($"Order item not found for ProductId {reviewInput.ProductId}. Skipping this review.");
                    continue;
                }

                var review = new Review
                {
                    UserId = userId.Value,
                    ProductId = orderItem.ProductId ?? throw new Exception("ProductId cannot be null"),
                    Rating = reviewInput.Rating,
                    Comment = reviewInput.Comment,
                    OrderId = order.Id,
                    ReviewDate = DateTime.UtcNow,
                    HasReviewed = true,
                    IsApproved = true // Giả sử phê duyệt ban đầu là `true`
                };

                await _reviewRepository.CreateAsync(review);
                await _reviewRepository.SaveChangesAsync();

                // Cập nhật lại Product's TotalRating, ReviewCount, và AverageRating sau khi thêm Review
                await _productService.UpdateProductAverageRating(review.ProductId);

                var reviewDto = _mapper.Map<ReviewDto>(review);
                reviewDtos.Add(reviewDto);
                reviewedOrder = order;
            }

            if (reviewedOrder != null)
            {
                reviewedOrder.HasReviewe = true;
                await _orderRepository.UpdateAsync(reviewedOrder);
                await _orderRepository.SaveChangesAsync();
            }

            return reviewDtos;
        }

        public async Task<ReviewDto> ChangeApprovalStatus(ChangeStatusReview input)
        {
            // Tìm review theo ID
            var review = await _reviewRepository.GetAsync(input.ReviewId);
            if (review == null)
            {
                throw new Exception("Review not found.");
            }

            // Kiểm tra xem trạng thái phê duyệt có thay đổi từ false sang true không
            bool statusReview = !review.IsApproved && input.Approve;

            // Thay đổi trạng thái phê duyệt dựa trên tham số approve
            review.IsApproved = input.Approve;

            // Cập nhật review trong cơ sở dữ liệu
            await _reviewRepository.UpdateAsync(review);
            await _reviewRepository.SaveChangesAsync();

            // Nếu trạng thái chuyển từ unapproved sang approved, cập nhật AverageRating
            if (statusReview)
            {
                await _productService.UpdateProductAverageRating(review.ProductId);
            }

            // Trả về thông tin review đã được chuyển đổi trạng thái
            return _mapper.Map<ReviewDto>(review);
        }


        //public async Task<List<ReviewDto>> CreateReviews(List<CreateReviewDto> input)
        //{
        //    // Lấy UserId của người dùng hiện tại
        //    var userId = _currentUser.Id;
        //    if (userId == null)
        //    {
        //        throw new Exception("User not authenticated");
        //    }

        //    var reviewDtos = new List<ReviewDto>();

        //    foreach (var reviewInput in input)
        //    {
        //        // Lấy OrderId từ CreateReviewDto và kiểm tra trạng thái Completed
        //        var order = await _orderRepository
        //            .AsQueryable()
        //            .Include(o => o.OrderItems)
        //            .FirstOrDefaultAsync(o => o.Id == reviewInput.OrderId && o.UserId == userId && o.OrderStatus == Enums.OrderStatus.Completed);

        //        if (order == null)
        //        {
        //            _logger.LogWarning($"No completed order found for Order ID: {reviewInput.OrderId}. Skipping this review.");
        //            continue;
        //        }

        //        // Tìm OrderItem phù hợp dựa trên ProductId và VariantId từ CreateReviewDto
        //        var orderItem = order.OrderItems.FirstOrDefault(item => item.ProductId == reviewInput.ProductId && item.VariantId == reviewInput.VariantId);

        //        if (orderItem == null)
        //        {
        //            _logger.LogWarning($"Order item not found for ProductId {reviewInput.ProductId} and VariantId {reviewInput.VariantId}. Skipping this review.");
        //            continue;
        //        }

        //        // Tạo đối tượng Review mới từ CreateReviewDto và thông tin OrderItem
        //        var review = new Review
        //        {
        //            UserId = userId.Value,
        //            ProductId = orderItem.ProductId ?? throw new Exception("ProductId cannot be null"),
        //            VariantId = orderItem.VariantId, // Cho phép null nếu không có VariantId
        //            Rating = reviewInput.Rating,
        //            Comment = reviewInput.Comment,
        //            OrderId = order.Id,
        //            ReviewDate = DateTime.UtcNow,
        //            HasReviewed = true,
        //            IsApproved = false // Phê duyệt ban đầu là false
        //        };

        //        // Lưu đối tượng Review vào cơ sở dữ liệu
        //        await _reviewRepository.CreateAsync(review);
        //        await _reviewRepository.SaveChangesAsync();

        //        // Chuyển đổi Review sang ReviewDto để trả về
        //        var reviewDto = _mapper.Map<ReviewDto>(review);
        //        reviewDtos.Add(reviewDto);
        //    }

        //    return reviewDtos;
        //}



        public async Task<PagedViewModel<ReviewDto>> GetPagedReviews(ReviewGetRequestInputDto input)
        {
            // Bắt đầu từ truy vấn cơ bản và bao gồm các liên kết
            var query = _reviewRepository.AsQueryable();
              

            // Lọc theo ngày bắt đầu
            if (input.StartDate != null)
            {
                query = query.Where(r => r.ReviewDate >= input.StartDate);
            }

            // Lọc theo ngày kết thúc
            if (input.EndDate != null)
            {
                query = query.Where(r => r.ReviewDate <= input.EndDate);
            }

            // Lọc theo trạng thái phê duyệt
            if (input.IsApproved != null)
            {
                query = query.Where(r => r.IsApproved == input.IsApproved);
            }

            // Đếm tổng số bản ghi thỏa mãn điều kiện
            var totalCount = await query.CountAsync();

            // Áp dụng phân trang
            query = query.Skip((input.PageIndex - 1) * input.PageSize)
                         .Take(input.PageSize);

            // Chọn ra các trường cần thiết cho ReviewDto, UserDto và ProductDto
            var selectedReviews = query
                .Include(r => r.User)      
                .Include(r => r.Product)   
                .Select(r => new ReviewDto
                {
                Id = r.Id,
                UserId = r.UserId,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewDate = r.ReviewDate,
                IsApproved = r.IsApproved,
                HasReviewed = r.HasReviewed,
                User = new UserDto
                {
                    FullName = r.User.FullName,
                    Image = r.User.Image
                },
                Product = new ProductDto
                {
                    ProductName = r.Product.ProductName,
                    Variants = r.Variant != null ? new List<ProductVariantDto>
            {
                new ProductVariantDto { NameVariant = r.Variant.NameVariant }
            } : new List<ProductVariantDto>()
                },
                ProductId = r.ProductId,
                VariantId = r.VariantId
            });

            // Lấy dữ liệu sau khi phân trang
            var reviewDtos = await selectedReviews.ToListAsync();

            // Trả về kết quả dưới dạng `PagedViewModel`
            return new PagedViewModel<ReviewDto>
            {
                Items = reviewDtos,
                TotalRecord = totalCount
            };
        }


        public async Task<PagedViewModel<ReviewDto>> GetReviewProduct(ReviewGetRequestInputDto input)
        {
            // Bắt đầu từ truy vấn cơ bản và bao gồm các liên kết
            var query = _reviewRepository.AsQueryable();

            // Lọc theo ProductId nếu có
            if (input.ProductId != null)
            {
                query = query.Where(r => r.ProductId == input.ProductId);
            }

            // Mặc định lọc theo trạng thái phê duyệt là true nếu IsApproved không được chỉ định
            if (input.IsApproved == null || input.IsApproved == true)
            {
                query = query.Where(r => r.IsApproved == true);
            }
            else
            {
                query = query.Where(r => r.IsApproved == false);
            }

            // Đếm tổng số bản ghi thỏa mãn điều kiện
            var totalCount = await query.CountAsync();

            // Áp dụng phân trang
            query = query.Skip((input.PageIndex - 1) * input.PageSize)
                         .Take(input.PageSize);

            // Chọn ra các trường cần thiết cho ReviewDto, UserDto và ProductDto
            var selectedReviews = query
                .Include(r => r.User)
                .Include(r => r.Product)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    IsApproved = r.IsApproved,
                    HasReviewed = r.HasReviewed,
                    User = new UserDto
                    {
                        FullName = r.User.FullName,
                        Image = r.User.Image
                    },
                    Product = new ProductDto
                    {
                        ProductName = r.Product.ProductName,
                        Variants = r.Variant != null
                            ? new List<ProductVariantDto> { new ProductVariantDto { NameVariant = r.Variant.NameVariant } }
                            : new List<ProductVariantDto>()
                    },
                    ProductId = r.ProductId,
                    VariantId = r.VariantId
                });

            // Lấy dữ liệu sau khi phân trang
            var reviewDtos = await selectedReviews.ToListAsync();

            // Trả về kết quả dưới dạng `PagedViewModel`
            return new PagedViewModel<ReviewDto>
            {
                Items = reviewDtos,
                TotalRecord = totalCount
            };
        }

      

    }
}
