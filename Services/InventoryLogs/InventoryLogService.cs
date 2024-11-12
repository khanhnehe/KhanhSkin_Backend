using AutoMapper;
using KhanhSkin_BackEnd.Dtos.InventoryLog;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.EntityFrameworkCore;

namespace KhanhSkin_BackEnd.Services.InventoryLogs
{
    public class InventoryLogService: BaseService<KhanhSkin_BackEnd.Entities.InventoryLog, InventoryLogDto, InventoryLogDto, InventoryLogGetRequestInput>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<KhanhSkin_BackEnd.Entities.InventoryLog> _inventoryLogRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryLogService> _logger;
        private readonly ICurrentUser _currentUser;

        public InventoryLogService(
           IConfiguration config,
           IRepository<KhanhSkin_BackEnd.Entities.InventoryLog> inventoryLogRepository,
           IRepository<Product> productRepository,
           IRepository<ProductVariant> productVariantRepository,
           IMapper mapper,
           ILogger<InventoryLogService> logger,
           ICurrentUser currentUser)
           : base(mapper, inventoryLogRepository, logger, currentUser)
        {
            _config = config;
            _inventoryLogRepository = inventoryLogRepository;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;
        }


        public async Task<PagedViewModel<InventoryLogDto>> GetPagedInventoryLogs(InventoryLogGetRequestInput input)
        {
            // Khởi tạo truy vấn cơ bản
            var query = _inventoryLogRepository.AsQueryable();

            // Lọc theo ActionType nếu có
            if (input.ActionType.HasValue)
            {
                query = query.Where(log => log.ActionType == input.ActionType.Value);
            }

            // Lọc theo StartDate và EndDate nếu có
            if (input.StartDate.HasValue)
            {
                query = query.Where(log => log.TransactionDate >= input.StartDate.Value);
            }
            if (input.EndDate.HasValue)
            {
                query = query.Where(log => log.TransactionDate <= input.EndDate.Value);
            }

            // Kiểm tra nếu có giá trị FreeTextSearch, tìm theo ProductName và VariantName
            if (!string.IsNullOrEmpty(input.FreeTextSearch))
            {
                query = query.Where(log => log.ProductName.Contains(input.FreeTextSearch) ||
                                           log.VariantName.Contains(input.FreeTextSearch));
            }

            // Đếm tổng số bản ghi sau khi lọc
            var totalRecord = await query.CountAsync();

            // Áp dụng phân trang
            query = query
                .OrderByDescending(log => log.TransactionDate) // Sắp xếp giảm dần theo ngày giao dịch
                .Skip((input.PageIndex - 1) * input.PageSize)
                .Take(input.PageSize);

            // Lấy dữ liệu sau khi phân trang và ánh xạ sang InventoryLogDto
            var inventoryLogs = await query.ToListAsync();
            var inventoryLogDtos = _mapper.Map<List<InventoryLogDto>>(inventoryLogs);

            // Trả về kết quả dưới dạng PagedViewModel
            return new PagedViewModel<InventoryLogDto>
            {
                Items = inventoryLogDtos,
                TotalRecord = totalRecord
            };
        }


    }
}
