using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Voucher;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Services;
using static KhanhSkin_BackEnd.Consts.Enums;
using Microsoft.EntityFrameworkCore;
using KhanhSkin_BackEnd.Dtos.Supplier;
using KhanhSkin_BackEnd.Share.Dtos;

public class VoucherService : BaseService<KhanhSkin_BackEnd.Entities.Voucher, VoucherDto, CreateUpdateVoucherDto, VoucherGetRequestInputDto>
{
    private readonly IConfiguration _config;
    private readonly IRepository<KhanhSkin_BackEnd.Entities.Voucher> _voucherRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<VoucherService> _logger;

    public VoucherService(
        IConfiguration config,
        IRepository<KhanhSkin_BackEnd.Entities.Voucher> repository,
        IRepository<Product> productRepository,
        IMapper mapper,
        ILogger<VoucherService> logger,
        ICurrentUser currentUser)
        : base(mapper, repository, logger, currentUser)
    {
        _config = config;
        _voucherRepository = repository;
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<bool> CheckCodeExist(string code)
    {
        return await _voucherRepository.AsQueryable().AnyAsync(u => u.Code == code);
    }

    public override async Task<Voucher> Create(CreateUpdateVoucherDto input)
    {
        if (await CheckCodeExist(input.Code))
        {
            throw new ApiException("Mã voucher đã tồn tại.");
        }

        if (input.DiscountType == DiscountType.AmountMoney && input.DiscountValue <= 10000)
        {
            throw new ApiException("Giá trị phải lớn hơn 10000 cho loại giảm giá theo số tiền.");
        }
        if (input.DiscountType == DiscountType.Percentage && input.DiscountValue >= 70)
        {
            throw new ApiException("Giá trị phải nhỏ hơn 70 cho loại giảm giá theo phần trăm.");
        }

        if (input.DiscountType == DiscountType.Percentage && input.DiscountValue <= 5)
        {
            throw new ApiException("Giá trị phải lớn hơn 5%  cho loại giảm giá theo phần trăm.");
        }

        if (input.StartTime > input.EndTime)
        {
            throw new ApiException("Thời gian bắt đầu không được lớn hơn thời gian kết thúc.");
        }


        // Kiểm tra loại voucher và danh sách sản phẩm áp dụng
        if (input.VoucherType == VoucherType.Specific)
        {
            if (input.ProductVouchers == null || !input.ProductVouchers.Any())
            {
                throw new ApiException("Danh sách sản phẩm áp dụng không được để trống cho loại voucher cụ thể.");
            }
        }

        // Sử dụng AutoMapper để ánh xạ từ DTO sang thực thể Voucher
        var voucher = _mapper.Map<Voucher>(input);

        if (input.VoucherType == VoucherType.Specific && input.ProductVouchers != null && input.ProductVouchers.Any())
        {
            voucher.ProductVouchers = new List<ProductVoucher>();

            foreach (var productVoucherDto in input.ProductVouchers)
            {
                var product = await _productRepository.GetAsync(productVoucherDto.ProductId);
                if (product == null)
                {
                    throw new ApiException($"Không tìm thấy sản phẩm với ID {productVoucherDto.ProductId}.");
                }

                // Tạo ProductVoucher và thêm vào danh sách ProductVouchers của Voucher
                var productVoucher = new ProductVoucher
                {
                    Id = Guid.NewGuid(), // Khởi tạo Id mới
                    ProductId = product.Id,
                    VoucherId = voucher.Id
                };
                voucher.ProductVouchers.Add(productVoucher);
            }
        }

        await _voucherRepository.CreateAsync(voucher);
        await _voucherRepository.SaveChangesAsync();

        return voucher;
    }


    public override async Task<Voucher> Update(Guid id, CreateUpdateVoucherDto input)
    {
        try
        {
            var voucher = await _voucherRepository.AsQueryable()
                .Include(v => v.ProductVouchers)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (voucher == null)
            {
                _logger.LogError("Voucher with id {Id} not found", id);
                throw new KeyNotFoundException($"Voucher with id {id} not found");
            }

            if (input.DiscountType == DiscountType.AmountMoney && input.DiscountValue <= 10000)
            {
                throw new ApiException("Giá trị giảm giá phải lớn hơn 10000 cho loại giảm giá theo số tiền.");
            }
            if (input.DiscountType == DiscountType.Percentage && input.DiscountValue >= 70)
            {
                throw new ApiException("Giá trị giảm giá phải nhỏ hơn 70 cho loại giảm giá theo phần trăm.");
            }
            if (input.StartTime > input.EndTime)
            {
                throw new ApiException("Thời gian bắt đầu không được lớn hơn thời gian kết thúc.");
            }


            // Kiểm tra loại voucher và danh sách sản phẩm áp dụng
            if (input.VoucherType == VoucherType.Specific)
            {
                if (input.ProductVouchers == null || !input.ProductVouchers.Any())
                {
                    throw new ApiException("Danh sách sản phẩm áp dụng không được để trống cho loại voucher cụ thể.");
                }
            }

            // Ánh xạ các thuộc tính từ DTO sang thực thể Voucher (trừ ProductVouchers)
            _mapper.Map(input, voucher);

            if (input.VoucherType == VoucherType.Specific && input.ProductVouchers != null && input.ProductVouchers.Any())
            {
                var existProductVoucher = voucher.ProductVouchers.ToList();

                // Tìm các ProductVouchers cần thêm mới
                var newProductVoucher = input.ProductVouchers
                    .Where(pv => !existProductVoucher.Any(epv => epv.ProductId == pv.ProductId))
                    .Select(pv => new ProductVoucher
                    {
                        Id = Guid.NewGuid(),
                        ProductId = pv.ProductId,
                        VoucherId = voucher.Id
                    }).ToList();

                // Tìm các ProductVouchers cần xóa
                var removeProductVoucher = existProductVoucher
                    .Where(epv => !input.ProductVouchers.Any(pv => pv.ProductId == epv.ProductId))
                    .ToList();

                // Thêm các ProductVouchers mới vào voucher
                foreach (var productVoucher in newProductVoucher)
                {
                    voucher.ProductVouchers.Add(productVoucher);
                }

                // Xóa các ProductVouchers không còn tồn tại trong danh sách mới
                foreach (var productVoucher in removeProductVoucher)
                {
                    voucher.ProductVouchers.Remove(productVoucher);
                }
            }
            else
            {
                // Nếu không có ProductVouchers hoặc VoucherType không phải Specific, xóa tất cả ProductVouchers
                voucher.ProductVouchers.Clear();
            }

            await _voucherRepository.UpdateAsync(voucher);
            await _voucherRepository.SaveChangesAsync();

            return voucher;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating voucher: {VoucherId}", id);
            throw;
        }
    }

   



    public override async Task<VoucherDto> Get(Guid id)
    {
        try
        {
            var voucher = await _voucherRepository.AsQueryable()
                .Include(v => v.ProductVouchers)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (voucher == null)
            {
                _logger.LogError("Voucher with id {Id} not found", id);
                throw new KeyNotFoundException($"Voucher with id {id} not found");
            }

            return _mapper.Map<VoucherDto>(voucher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving voucher: {VoucherId}", id);
            throw;
        }
    }

    public override async Task<List<VoucherDto>> GetAll()
    {
        try
        {
            var vouchers = await _voucherRepository.AsQueryable()
                .Include(v => v.ProductVouchers)
                .ThenInclude(pv => pv.Product)
                .ToListAsync();

            return vouchers.Select(v => _mapper.Map<VoucherDto>(v)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all vouchers");
            throw;
        }
    }


    public async Task<List<VoucherDto>> GetVoucherActive()
    {
        try
        {
            var currentDate = DateTime.Now;

            var vouchers = await _voucherRepository.AsQueryable()
                .Include(v => v.ProductVouchers)
                .ThenInclude(pv => pv.Product)
                .Where(v => v.EndTime > currentDate && v.TotalUses >= 1 && v.IsActive)
                .ToListAsync();

            return vouchers.Select(v => _mapper.Map<VoucherDto>(v)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all vouchers");
            throw;
        }
    }


    public override async Task<Voucher> Delete(Guid id)
    {
        try
        {
            var voucher = await _voucherRepository.GetAsync(id);
            if (voucher == null)
            {
                _logger.LogError("Voucher with id {Id} not found", id);
                throw new KeyNotFoundException($"Voucher with id {id} not found");
            }

            await _voucherRepository.DeleteAsync(id);
            return voucher;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting voucher: {VoucherId}", id);
            throw;
        }
    }


    public IQueryable<Voucher> GetVoucherByFilter(VoucherGetRequestInputDto input)
    {
        // Bắt đầu truy vấn với tất cả các voucher
        var query = _voucherRepository.AsQueryable()
            .Include(v => v.ProductVouchers)
            .ThenInclude(pv => pv.Product)
            .AsNoTracking();

        // Sắp xếp theo thời gian bắt đầu giảm dần
        query = query.OrderByDescending(v => v.StartTime);

        // Kiểm tra trạng thái voucher: Còn hoạt động hay không
        if (!string.IsNullOrEmpty(input.Status))
        {
            if (input.Status.ToLower() == "active")
            {
                query = query.Where(v => v.IsActive == true &&
                                         v.EndTime > DateTime.Now &&
                                         v.TotalUses > 0);
            }
            else if (input.Status.ToLower() == "inactive")
            {
                query = query.Where(v => v.IsActive == false ||
                                         v.EndTime <= DateTime.Now ||
                                         v.TotalUses == 0);
            }
        }

        // Kiểm tra nếu người dùng muốn lọc theo loại voucher
        if (input.VoucherType.HasValue)
        {
            query = query.Where(v => v.VoucherType == input.VoucherType.Value);
        }

        // Lọc theo khoảng thời gian bắt đầu
        if (input.StartDate.HasValue)
        {
            query = query.Where(v => v.StartTime >= input.StartDate.Value);
        }

        // Lọc theo khoảng thời gian kết thúc
        if (input.EndDate.HasValue)
        {
            query = query.Where(v => v.EndTime <= input.EndDate.Value);
        }

        // Lọc bằng tìm kiếm tự do (FreeTextSearch)
        if (!string.IsNullOrEmpty(input.FreeTextSearch))
        {
            query = query.Where(v => v.ProgramName.Contains(input.FreeTextSearch) ||
                                     v.Description.Contains(input.FreeTextSearch) ||
                                     v.Code.Contains(input.FreeTextSearch));
        }

        return query; // Trả về IQueryable<Voucher>
    }


    public virtual async Task<PagedViewModel<Voucher>> GetPagedVouchers(VoucherGetRequestInputDto input)
    {
        // Bắt đầu từ truy vấn cơ bản
        var query = GetVoucherByFilter(input);

        // Đếm tổng số bản ghi thỏa mãn điều kiện
        var totalCount = await query.CountAsync();

        // Áp dụng phân trang
        query = query.Skip((input.PageIndex - 1) * input.PageSize)
                     .Take(input.PageSize);

        // Lấy dữ liệu sau khi phân trang
        var data = await query.ToListAsync();

        // Trả về kết quả dưới dạng `PagedViewModel`
        return new PagedViewModel<Voucher>
        {
            Items = data,
            TotalRecord = totalCount
        };
    }





}