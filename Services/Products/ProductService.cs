using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Dtos.ProductVariant;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace KhanhSkin_BackEnd.Services.Products
{
    public class ProductService : BaseService<Product, ProductDto, CreateUpdateProductDto, ProductGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductType> _productTypeRepository;
        private readonly IRepository<Brand> _brandRepository;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IConfiguration config,
            IRepository<Product> repository,
            IRepository<Category> categoryRepository,
            IRepository<ProductType> productTypeRepository,
            IRepository<Brand> brandRepository,
            IRepository<ProductVariant> productVariantRepository,
            IMapper mapper,
            ILogger<ProductService> logger,
            ICurrentUser currentUser)
            : base(mapper, repository, logger, currentUser)
        {
            _config = config;
            _productRepository = repository;
            _categoryRepository = categoryRepository;
            _productTypeRepository = productTypeRepository;
            _brandRepository = brandRepository;
            _productVariantRepository = productVariantRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> CheckProductExist(string productName)
        {
            return await _productRepository.AsQueryable().AnyAsync(u => u.ProductName == productName);
        }


        public async Task<bool> CheckVariantExist(string nameVariant, string skuVariant, Guid? productId = null)
        {
            // Kt biến thể tồn tại hay ko dựa trên NameVariant hoặc SKU của biến thể
            if (productId.HasValue)
            {
                // Nếu có productId, kt trong bối cảnh các biến thể khác với productId hiện tại
                return await _productVariantRepository.AsQueryable()
                    .AnyAsync(u => (u.NameVariant == nameVariant || u.SKUVariant == skuVariant) && u.ProductId != productId);
            }
            // Nếu không có productId, kt trong all biến thể
            return await _productVariantRepository.AsQueryable()
                .AnyAsync(u => u.NameVariant == nameVariant || u.SKUVariant == skuVariant);
        }


        public override async Task<Product> Create(CreateUpdateProductDto input)
        {
            if (await CheckProductExist(input.ProductName))
            {
                throw new ApiException("Sản phẩm đã tồn tại.");
            }

            if (await _productRepository.AsQueryable().AnyAsync(p => p.SKU == input.SKU))
            {
                throw new ApiException("SKU đã tồn tại.");
            }

            var brand = await _brandRepository.GetAsync(input.BrandId);
            if (brand == null)
            {
                throw new ApiException("Không tìm thấy thương hiệu.");
            }

            // Kt tồn tại all Categories
            var categories = new List<Category>();
            foreach (var categoryId in input.CategoryIds)
            {
                var category = await _categoryRepository.GetAsync(categoryId);
                if (category == null)
                {
                    throw new ApiException($"Không tìm thấy danh mục.");
                }
                categories.Add(category);
            }

            // Kt tồn tại của all ProductTypes
            var productTypes = new List<ProductType>();
            foreach (var productTypeId in input.ProductTypeIds)
            {
                var productType = await _productTypeRepository.GetAsync(productTypeId);
                if (productType == null)
                {
                    throw new ApiException($"Không tìm thấy loại sản phẩm.");
                }
                productTypes.Add(productType);
            }

            // Kt xử lý các biến thể sp nếu có
            var variants = new List<ProductVariant>();
            if (input.Variants != null && input.Variants.Any())
            {
                // Tính total của các biến thể
                var totalVariantQuantity = input.Variants.Sum(v => v.QuantityVariant);

                // Kt nếu total của các biến thể ko = Quantity của sp
                if (totalVariantQuantity != input.Quantity)
                {
                    throw new ApiException("Tổng số lượng biến thể phải bằng với Quantity của sản phẩm.");
                }

              // Kt sự trùng lặp trong ds biến thể đầu vào
                var variantSKUs = new HashSet<string>();
                foreach (var variantDto in input.Variants)
                {
                    if (variantSKUs.Contains(variantDto.SKUVariant))
                    {
                        throw new ApiException("Các biến thể trong danh sách đầu vào không được có SKU trùng lặp.");
                    }
                    variantSKUs.Add(variantDto.SKUVariant);

                    // Kt tồn tại của biến thể trong db
                    if (await CheckVariantExist(variantDto.NameVariant, variantDto.SKUVariant, null))
                    {
                        throw new ApiException("Biến thể đã tồn tại cho sản phẩm này hoặc SKU biến thể đã tồn tại.");
                    }

                    var variant = _mapper.Map<ProductVariant>(variantDto);

                    if (variant.DiscountVariant.HasValue && variant.DiscountVariant.Value > 0 && variant.DiscountVariant.Value <= 100)
                    {
                        variant.SalePriceVariant = variant.PriceVariant - (variant.PriceVariant * variant.DiscountVariant.Value / 100);
                    }
                    else
                    {
                        variant.SalePriceVariant = variant.PriceVariant;
                    }

                    variant.Id = Guid.NewGuid();
                    variants.Add(variant);
                }
            }

            // Chuyển đổi DTO thành thực thể Product
            var product = _mapper.Map<Product>(input);
            product.BrandId = input.BrandId;
            product.Categories = categories;
            product.ProductTypes = productTypes;

            // Tính SalePrice của sp nếu có Discount
            if (product.Discount.HasValue && product.Discount.Value > 0 && product.Discount.Value <= 100)
            {
                product.SalePrice = product.Price - (product.Price * product.Discount.Value / 100);
            }
            else
            {
                product.SalePrice = product.Price;
            }

            // Thêm sp vào db
            await _productRepository.CreateAsync(product);
            await _productRepository.SaveChangesAsync();

            // Thêm các biến thể vào db
            foreach (var variant in variants)
            {
                variant.ProductId = product.Id;
                await _productVariantRepository.CreateAsync(variant);
            }

            await _productVariantRepository.SaveChangesAsync();

            return product;
        }


        public override async Task<Product> Update(Guid id, CreateUpdateProductDto input)
        {
            try
            {
                // Tìm sản phẩm theo ID
                var product = await _productRepository.AsQueryable()
                    .Include(p => p.Categories) // Bao gồm các danh mục liên quan
                    .Include(p => p.ProductTypes) // Bao gồm các loại sản phẩm liên quan
                    .FirstOrDefaultAsync(a => a.Id == id); // Tìm sản phẩm theo ID
                if (product == null)
                {
                    _logger.LogError("Product with id {Id} not found", id);
                    throw new KeyNotFoundException($"Product with id {id} not found"); // Ném lỗi nếu không tìm thấy sản phẩm
                }

                // Kiểm tra SKU có tồn tại nhưng không phải của sản phẩm hiện tại
                if (await _productRepository.AsQueryable().AnyAsync(p => p.SKU == input.SKU && p.Id != id))
                {
                    throw new ApiException("SKU đã tồn tại."); // Ném lỗi nếu SKU đã tồn tại
                }

                // Kiểm tra sự tồn tại của Brand dựa trên BrandId
                var brand = await _brandRepository.GetAsync(input.BrandId);
                if (brand == null)
                {
                    throw new ApiException($"Không tìm thấy thương hiệu với id {input.BrandId}."); // Ném lỗi nếu không tìm thấy thương hiệu
                }
                product.BrandId = input.BrandId; // Cập nhật BrandId cho sản phẩm

                // Chuyển đổi DTO thành thực thể Product và cập nhật dữ liệu
                _mapper.Map(input, product);

                // Tính toán SalePrice nếu có Discount
                if (product.Discount.HasValue && product.Discount.Value > 0 && product.Discount.Value <= 100)
                {
                    product.SalePrice = product.Price - (product.Price * product.Discount.Value / 100); // Tính giá bán sau khi giảm giá
                }
                else
                {
                    product.SalePrice = product.Price; // Nếu không có giảm giá, giá bán là giá gốc
                }

                // Cập nhật mối quan hệ nhiều-nhiều cho Categories
                var existingCategoryIds = product.Categories.Select(c => c.Id).ToList();
                var newCategoryIds = input.CategoryIds.Except(existingCategoryIds).ToList();
                var removedCategoryIds = existingCategoryIds.Except(input.CategoryIds).ToList();

                // Xóa các Category không còn thuộc sản phẩm
                foreach (var categoryId in removedCategoryIds)
                {
                    var categoryToRemove = product.Categories.FirstOrDefault(c => c.Id == categoryId);
                    if (categoryToRemove != null)
                    {
                        product.Categories.Remove(categoryToRemove); // Xóa danh mục không còn thuộc sản phẩm
                    }
                }

                // Thêm các Category mới vào sản phẩm
                foreach (var categoryId in newCategoryIds)
                {
                    var category = await _categoryRepository.GetAsync(categoryId);
                    if (category == null)
                    {
                        throw new ApiException($"Không tìm thấy danh mục với id {categoryId}."); // Ném lỗi nếu không tìm thấy danh mục
                    }
                    product.Categories.Add(category); // Thêm danh mục mới vào sản phẩm
                }

                // Cập nhật mối quan hệ nhiều-nhiều cho ProductTypes
                var existingProductTypeIds = product.ProductTypes.Select(pt => pt.Id).ToList();
                var newProductTypeIds = input.ProductTypeIds.Except(existingProductTypeIds).ToList();
                var removedProductTypeIds = existingProductTypeIds.Except(input.ProductTypeIds).ToList();

                // Xóa các ProductType không còn thuộc sản phẩm
                foreach (var productTypeId in removedProductTypeIds)
                {
                    var productTypeToRemove = product.ProductTypes.FirstOrDefault(pt => pt.Id == productTypeId);
                    if (productTypeToRemove != null)
                    {
                        product.ProductTypes.Remove(productTypeToRemove); // Xóa loại sản phẩm không còn thuộc sản phẩm
                    }
                }

                // Thêm các ProductType mới vào sản phẩm
                foreach (var productTypeId in newProductTypeIds)
                {
                    var productType = await _productTypeRepository.GetAsync(productTypeId);
                    if (productType == null)
                    {
                        throw new ApiException($"Không tìm thấy loại sản phẩm với id {productTypeId}."); // Ném lỗi nếu không tìm thấy loại sản phẩm
                    }
                    product.ProductTypes.Add(productType); // Thêm loại sản phẩm mới vào sản phẩm
                }

                // Cập nhật sản phẩm trong cơ sở dữ liệu
                await _productRepository.UpdateAsync(product);

                // Xử lý các biến thể sản phẩm
                var existingVariants = await _productVariantRepository.AsQueryable().Where(v => v.ProductId == id).ToListAsync();
                _productVariantRepository.Table.RemoveRange(existingVariants); // Xóa các biến thể hiện tại
                await _productVariantRepository.SaveChangesAsync();

                var variants = new List<ProductVariant>();
                if (input.Variants != null && input.Variants.Any())
                {
                    var totalVariantQuantity = input.Variants.Sum(v => v.QuantityVariant);

                    if (totalVariantQuantity != input.Quantity)
                    {
                        throw new ApiException("Tổng số lượng biến thể phải bằng với Quantity của sản phẩm."); // Ném lỗi nếu tổng số lượng biến thể không khớp với số lượng sản phẩm
                    }

                    var variantSKUs = new HashSet<string>();
                    foreach (var variantDto in input.Variants)
                    {
                        if (variantSKUs.Contains(variantDto.SKUVariant))
                        {
                            throw new ApiException("Các biến thể trong danh sách đầu vào không được có SKU trùng lặp."); // Ném lỗi nếu có SKU trùng lặp
                        }
                        variantSKUs.Add(variantDto.SKUVariant);

                        if (await CheckVariantExist(variantDto.NameVariant, variantDto.SKUVariant, id))
                        {
                            throw new ApiException("Biến thể đã tồn tại cho sản phẩm này hoặc SKU biến thể đã tồn tại."); // Ném lỗi nếu biến thể đã tồn tại
                        }

                        var variant = _mapper.Map<ProductVariant>(variantDto);

                        if (variant.DiscountVariant.HasValue && variant.DiscountVariant.Value > 0 && variant.DiscountVariant.Value <= 100)
                        {
                            variant.SalePriceVariant = variant.PriceVariant - (variant.PriceVariant * variant.DiscountVariant.Value / 100); // Tính giá bán biến thể sau khi giảm giá
                        }
                        else
                        {
                            variant.SalePriceVariant = variant.PriceVariant; // Nếu không có giảm giá, giá bán biến thể là giá gốc
                        }

                        variant.Id = Guid.NewGuid(); // Tạo ID mới cho biến thể
                        variants.Add(variant);
                    }
                }

                foreach (var variant in variants)
                {
                    variant.ProductId = product.Id; // Gán ProductId cho biến thể
                    await _productVariantRepository.CreateAsync(variant); // Tạo biến thể mới
                }

                await _productVariantRepository.SaveChangesAsync(); // Lưu thay đổi

                return product; // Trả về sản phẩm đã cập nhật
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductName}", typeof(Product).Name); // Ghi log lỗi
                throw; // Ném lại lỗi
            }
        }



        public override async Task<Product> Delete(Guid id)
        {
            var product = await _productRepository.AsQueryable().FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new ApiException("Product not found.");
            }

            await _productRepository.DeleteAsync(id);

            return product;
        }

        public override async Task<List<ProductDto>> GetAll()
        {
            var products = await _productRepository.AsQueryable()
                .Include(p => p.Brand)
                .Include(p => p.Categories)
                .Include(p => p.ProductTypes)
                .Include(p => p.Variants)
                .ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);
        }


        public override async Task<ProductDto> Get(Guid id)
        {
            var product = await _productRepository.AsQueryable()
                .Include(p => p.Brand)
                .Include(p => p.Categories)
                .Include(p => p.ProductTypes)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new ApiException("Product not found.");
            }

            return _mapper.Map<ProductDto>(product);
        }

        public override IQueryable<Product> CreateFilteredQuery(ProductGetRequestInputDto input)
        {
            // Tạo một truy vấn cơ bản từ phương thức cơ sở
            var query = base.CreateFilteredQuery(input);

            // Kiểm tra nếu có BrandId để lọc các sản phẩm theo thương hiệu
            if (input.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == input.BrandId.Value);
            }

            // Kiểm tra nếu có CategoryIds để lọc các sản phẩm theo danh mục
            if (input.CategoryIds != null && input.CategoryIds.Any())
            {
                query = query.Where(p => p.Categories.Any(c => input.CategoryIds.Contains(c.Id)));
            }

            // Kiểm tra nếu có ProductTypeIds để lọc các sản phẩm theo loại sản phẩm
            if (input.ProductTypeIds != null && input.ProductTypeIds.Any())
            {
                query = query.Where(p => p.ProductTypes.Any(pt => input.ProductTypeIds.Contains(pt.Id)));
            }

            // Áp dụng phân trang và sắp xếp nếu có
            if (!string.IsNullOrWhiteSpace(input.Sort))
            {
                query = query.OrderBy(input.Sort);
            }

            if (input.PageIndex.HasValue && input.PageSize.HasValue)
            {
                query = query.Skip((input.PageIndex.Value - 1) * input.PageSize.Value).Take(input.PageSize.Value);
            }

            return query; // Trả về truy vấn đã được lọc
        }



        public async Task<List<ProductOutstandingDto>> Search(string keyword)
        {
            var products = await _productRepository
                .AsQueryable()
                .Where(p => p.ProductName.Contains(keyword) || p.SKU.Contains(keyword))
                .Include(p => p.Brand)
                .Include(p => p.Categories)
                .Include(p => p.ProductTypes)
                .Include(p => p.Variants)
                .ToListAsync();

            return _mapper.Map<List<ProductOutstandingDto>>(products);
        }


        public async Task<List<ProductOutstandingDto>> GetByCategory(Guid categoryId)
        {
            var products = await _productRepository.AsQueryable()
                .Where(p => p.Categories.Any(c => c.Id == categoryId))
                .Include(p => p.Brand)
                .Include(p => p.Categories)
                .Include(p => p.ProductTypes)
                .Include(p => p.Variants)
                .ToListAsync();

            return _mapper.Map<List<ProductOutstandingDto>>(products);
        }

        public async Task<List<ProductOutstandingDto>> GetByProductType(Guid productTypeId)
        {
            var products = await _productRepository.AsQueryable()
                .Where(p => p.ProductTypes.Any(pt => pt.Id == productTypeId))
                .Include(p => p.Brand)
                .Include(p => p.Categories)
                .Include(p => p.ProductTypes)
                .Include(p => p.Variants)
                .ToListAsync();

            return _mapper.Map<List<ProductOutstandingDto>>(products);
        }

        public async Task<List<ProductOutstandingDto>> GetByBrand(Guid brandId)
        {
            var products = await _productRepository.AsQueryable()
                .Where(p => p.BrandId == brandId)
                .Include(p => p.Brand)
                .Include(p => p.Categories)
                .Include(p => p.ProductTypes)
                .Include(p => p.Variants)
                .ToListAsync();

            return _mapper.Map<List<ProductOutstandingDto>>(products);
        }
    }

}
