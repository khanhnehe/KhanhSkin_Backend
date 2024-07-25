using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Product;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace KhanhSkin_BackEnd.Services.Products
{
    public class ProductService : BaseService<Product, ProductDto, CreateUpdateProductDto, ProductGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductType> _productTypeRepository;
        private readonly IRepository<Brand> _brandRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IConfiguration config,
            IRepository<Product> repository,
            IRepository<Category> categoryRepository,
            IRepository<ProductType> productTypeRepository,
            IRepository<Brand> brandRepository,
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
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> CheckProductExist(string productName)
        {
            return await _productRepository.AsQueryable().AnyAsync(u => u.ProductName == productName);
        }

        public override async Task<Product> Create(CreateUpdateProductDto input)
        {
            // Kiểm tra sản phẩm có tồn tại dựa trên tên sản phẩm
            if (await CheckProductExist(input.ProductName))
            {
                throw new ApiException("Sản phẩm đã tồn tại."); // Ném ngoại lệ nếu sản phẩm đã tồn tại
            }

            // Kiểm tra SKU có tồn tại hay không
            if (await _productRepository.AsQueryable().AnyAsync(p => p.SKU == input.SKU))
            {
                throw new ApiException("SKU đã tồn tại."); // Ném ngoại lệ nếu SKU đã tồn tại
            }

            // Chuyển đổi DTO thành thực thể Product
            var product = _mapper.Map<Product>(input);

            // Tính toán SalePrice nếu có Discount
            if (product.Discount.HasValue && product.Discount.Value > 0 && product.Discount.Value <= 100)
            {
                product.SalePrice = product.Price - (product.Price * product.Discount.Value / 100); // Tính SalePrice nếu có giảm giá
            }
            else
            {
                product.SalePrice = product.Price; // Nếu không có giảm giá, SalePrice bằng Price
            }

            // Kiểm tra sự tồn tại của Brand dựa trên BrandId
            var brand = await _brandRepository.GetAsync(input.BrandId);
            if (brand == null)
            {
                throw new ApiException("Không tìm thấy thương hiệu."); // Ném ngoại lệ nếu không tìm thấy Brand
            }
            product.BrandId = input.BrandId;

            // Xử lý các mối quan hệ nhiều-nhiều cho Categories
            product.Categories = new List<Category>();
            foreach (var categoryId in input.CategoryIds)
            {
                var category = await _categoryRepository.GetAsync(categoryId);
                if (category == null)
                {
                    throw new ApiException($"Không tìm thấy danh mục."); // Ném ngoại lệ nếu không tìm thấy Category
                }
                product.Categories.Add(category); // Thêm Category vào danh sách của sản phẩm
            }

            // Xử lý các mối quan hệ nhiều-nhiều cho ProductTypes
            product.ProductTypes = new List<ProductType>();
            foreach (var productTypeId in input.ProductTypeIds)
            {
                var productType = await _productTypeRepository.GetAsync(productTypeId);
                if (productType == null)
                {
                    throw new ApiException($"Không tìm thấy loại sản phẩm."); // Ném ngoại lệ nếu không tìm thấy ProductType
                }
                product.ProductTypes.Add(productType); // Thêm ProductType vào danh sách của sản phẩm
            }

            // Tạo mới sản phẩm trong cơ sở dữ liệu
            await _productRepository.CreateAsync(product);
            await _productRepository.SaveChangesAsync();

            return product; // Trả về sản phẩm vừa tạo
        }

        public override async Task<Product> Update(Guid id, CreateUpdateProductDto input)
        {
            // Tìm sản phẩm theo ID
            var product = await _productRepository.AsQueryable()
                .Include(p => p.Categories)
                .Include(p => p.ProductTypes)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (product == null)
            {
                throw new ApiException($"Không tìm thấy sản phẩm với id {id}."); // Ném ngoại lệ nếu không tìm thấy sản phẩm
            }

            // Kiểm tra SKU có tồn tại nhưng không phải của sản phẩm hiện tại
            if (await _productRepository.AsQueryable().AnyAsync(p => p.SKU == input.SKU && p.Id != id))
            {
                throw new ApiException("SKU đã tồn tại."); // Ném ngoại lệ nếu SKU đã tồn tại và không phải của sản phẩm hiện tại
            }

            // Kiểm tra sự tồn tại của Brand dựa trên BrandId
            var brand = await _brandRepository.GetAsync(input.BrandId);
            if (brand == null)
            {
                throw new ApiException($"Không tìm thấy thương hiệu với id {input.BrandId}."); // Ném ngoại lệ nếu không tìm thấy Brand
            }
            product.BrandId = input.BrandId;

            // Chuyển đổi DTO thành thực thể Product và cập nhật dữ liệu
            _mapper.Map(input, product);

            // Tính toán SalePrice nếu có Discount
            if (product.Discount.HasValue && product.Discount.Value > 0 && product.Discount.Value <= 100)
            {
                product.SalePrice = product.Price - (product.Price * product.Discount.Value / 100); // Tính SalePrice nếu có giảm giá
            }
            else
            {
                product.SalePrice = product.Price; // Nếu không có giảm giá, SalePrice bằng Price
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
                    product.Categories.Remove(categoryToRemove);
                }
            }

            // Thêm các Category mới vào sản phẩm
            foreach (var categoryId in newCategoryIds)
            {
                var category = await _categoryRepository.GetAsync(categoryId);
                if (category == null)
                {
                    throw new ApiException($"Không tìm thấy danh mục với id {categoryId}."); // Ném ngoại lệ nếu không tìm thấy Category
                }
                product.Categories.Add(category);
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
                    product.ProductTypes.Remove(productTypeToRemove);
                }
            }

            // Thêm các ProductType mới vào sản phẩm
            foreach (var productTypeId in newProductTypeIds)
            {
                var productType = await _productTypeRepository.GetAsync(productTypeId);
                if (productType == null)
                {
                    throw new ApiException($"Không tìm thấy loại sản phẩm với id {productTypeId}."); // Ném ngoại lệ nếu không tìm thấy ProductType
                }
                product.ProductTypes.Add(productType);
            }

            // Cập nhật sản phẩm trong cơ sở dữ liệu
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            return product; // Trả về sản phẩm vừa cập nhật
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
