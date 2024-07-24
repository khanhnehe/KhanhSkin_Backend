using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.ProductVariant;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace KhanhSkin_BackEnd.Services.ProductVariants
{
    public class ProductVariantService : BaseService<ProductVariant, ProductVariantDto, ProductVariantDto, ProductVariantGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<Product> _productRepository;
        private readonly ILogger<ProductVariantService> _logger;

        public ProductVariantService(
            IConfiguration config,
            IRepository<ProductVariant> repository,
            IMapper mapper,
            ILogger<ProductVariantService> logger,
            ICurrentUser currentUser,
            IRepository<Product> productRepository) // Inject repository for ProductType
            : base(mapper, repository, logger, currentUser)
        {
            _config = config;
            _productVariantRepository = repository;
            _mapper = mapper;
            _logger = logger;
            _productRepository = productRepository; // Assign injected repository
        }

        public async Task<bool> CheckVariantExist(string nameVariant, Guid productId)
        {
            return await _productVariantRepository.AsQueryable()
                .AnyAsync(u => u.NameVariant == nameVariant && u.ProductId == productId);
        }


        public override async Task<ProductVariant> Create(ProductVariantDto input)
        {
            // Kiểm tra sự tồn tại của biến thể dựa trên tên biến thể và ProductId
            if (await CheckVariantExist(input.NameVariant, input.ProductId))
            {
                throw new ApiException("Biến thể đã tồn tại cho sản phẩm này."); // Ném ngoại lệ nếu biến thể đã tồn tại trong sản phẩm này
            }

            // Kiểm tra SKU có tồn tại hay không
            if (await _productVariantRepository.AsQueryable().AnyAsync(p => p.SKUVariant == input.SKUVariant))
            {
                throw new ApiException("SKU đã tồn tại."); // Ném ngoại lệ nếu SKU đã tồn tại
            }

            // Kiểm tra sự tồn tại của Product dựa trên ProductId
            var product = await _productRepository.GetAsync(input.ProductId);
            if (product == null)
            {
                throw new ApiException("Không tìm thấy sản phẩm."); // Ném ngoại lệ nếu không tìm thấy Product
            }

            // Chuyển đổi DTO thành thực thể ProductVariant
            var variant = _mapper.Map<ProductVariant>(input);

            // Tính toán SalePrice nếu có Discount
            if (variant.DiscountVariant.HasValue && variant.DiscountVariant.Value > 0 && variant.DiscountVariant.Value <= 100)
            {
                variant.SalePriceVariant = variant.PriceVariant - (variant.PriceVariant * variant.DiscountVariant.Value / 100); // Tính SalePrice nếu có giảm giá
            }
            else
            {
                variant.SalePriceVariant = variant.PriceVariant; // Nếu không có giảm giá, SalePrice bằng Price
            }

            // Thiết lập quan hệ giữa ProductVariant và Product
            variant.ProductId = product.Id;

            // Đảm bảo rằng id của biến thể là duy nhất
            variant.Id = Guid.NewGuid();

            // Tạo mới ProductVariant trong cơ sở dữ liệu
            await _productVariantRepository.CreateAsync(variant);
            await _productVariantRepository.SaveChangesAsync();

            return variant; // Trả về ProductVariant vừa tạo
        }



        public override async Task<ProductVariant> Update(Guid id, ProductVariantDto input)
        {
            // Tìm biến thể theo ID
            var variant = await _productVariantRepository.AsQueryable()
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (variant == null)
            {
                throw new ApiException($"Không tìm thấy biến thể với id {id}."); // Ném ngoại lệ nếu không tìm thấy biến thể
            }

            // Kiểm tra sự tồn tại của biến thể dựa trên tên biến thể và ProductId
            if (await CheckVariantExist(input.NameVariant, input.ProductId))
            {
                throw new ApiException("Biến thể đã tồn tại cho sản phẩm này."); // Ném ngoại lệ nếu biến thể đã tồn tại trong sản phẩm này
            }

            // Kiểm tra SKU có tồn tại nhưng không phải của biến thể hiện tại
            if (await _productVariantRepository.AsQueryable().AnyAsync(p => p.SKUVariant == input.SKUVariant && p.Id != id))
            {
                throw new ApiException("SKU đã tồn tại."); // Ném ngoại lệ nếu SKU đã tồn tại và không phải của biến thể hiện tại
            }

            // Kiểm tra sự tồn tại của Product dựa trên ProductId
            var product = await _productRepository.GetAsync(input.ProductId);
            if (product == null)
            {
                throw new ApiException("Không tìm thấy sản phẩm."); // Ném ngoại lệ nếu không tìm thấy Product
            }

            // Chuyển đổi DTO thành thực thể ProductVariant và cập nhật dữ liệu
            _mapper.Map(input, variant);

            // Tính toán SalePrice nếu có Discount
            if (variant.DiscountVariant.HasValue && variant.DiscountVariant.Value > 0 && variant.DiscountVariant.Value <= 100)
            {
                variant.SalePriceVariant = variant.PriceVariant - (variant.PriceVariant * variant.DiscountVariant.Value / 100); // Tính SalePrice nếu có giảm giá
            }
            else
            {
                variant.SalePriceVariant = variant.PriceVariant; // Nếu không có giảm giá, SalePrice bằng Price
            }

            // Cập nhật quan hệ giữa ProductVariant và Product
            variant.ProductId = product.Id;

            // Cập nhật ProductVariant trong cơ sở dữ liệu
            await _productVariantRepository.UpdateAsync(variant);
            await _productVariantRepository.SaveChangesAsync();

            return variant; // Trả về ProductVariant vừa cập nhật
        }


        public override async Task<ProductVariantDto> Get(Guid id)
        {
            // Tải biến thể sản phẩm cùng với các quan hệ liên quan
            var variant = await _productVariantRepository.AsQueryable()
                .Include(v => v.Product) // Bao gồm quan hệ với Product
                .FirstOrDefaultAsync(v => v.Id == id);

            if (variant == null)
            {
                throw new ApiException("Variant not found.");
            }

            // Ánh xạ thực thể sang DTO
            return _mapper.Map<ProductVariantDto>(variant);
        }

        public override async Task<List<ProductVariantDto>> GetAll()
        {
            // Tải tất cả biến thể sản phẩm cùng với các quan hệ liên quan
            var variants = await _productVariantRepository.AsQueryable()
                .Include(v => v.Product) // Bao gồm quan hệ với Product
                .ToListAsync();

            // Ánh xạ danh sách thực thể sang danh sách DTO
            return _mapper.Map<List<ProductVariantDto>>(variants);
        }

        public override async Task<ProductVariant> Delete(Guid id)
        {
            var variant = await _productVariantRepository.AsQueryable().FirstOrDefaultAsync(v => v.Id == id);
            if (variant == null)
            {
                throw new ApiException("Product variant not found.");
            }

            await _productVariantRepository.DeleteAsync(id);

            return variant;
        }



    }
}
