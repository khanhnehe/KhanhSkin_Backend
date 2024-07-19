using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Product;
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
            if (await CheckProductExist(input.ProductName))
            {
                throw new ApiException("Sản phẩm đã tồn tại.");
            }

            var product = _mapper.Map<Product>(input);

            // Kiểm tra sự tồn tại của Brand
            var brand = await _brandRepository.GetAsync(input.BrandId);
            if (brand == null)
            {
                throw new ApiException($"Không tìm thấy thương hiệu.");
            }
            product.BrandId = input.BrandId;

            // Xử lý các mối quan hệ nhiều-nhiều cho Categories và ProductTypes
            product.Categories = new List<Category>();
            foreach (var categoryId in input.CategoryIds)
            {
                var category = await _categoryRepository.GetAsync(categoryId);
                if (category == null)
                {
                    throw new ApiException($"Không tìm thấy danh mục.");
                }
                product.Categories.Add(category);
            }

            product.ProductTypes = new List<ProductType>();
            foreach (var productTypeId in input.ProductTypeIds)
            {
                var productType = await _productTypeRepository.GetAsync(productTypeId);
                if (productType == null)
                {
                    throw new ApiException($"Không tìm thấy loại sản phẩm.");
                }
                product.ProductTypes.Add(productType);
            }

            // Kiểm tra sự tồn tại của sản phẩm với ID cụ thể
            var existingProduct = await _productRepository.GetAsync(product.Id);
            if (existingProduct != null)
            {
                throw new ApiException($"Sản phẩm  đã tồn tại.");
            }

            await _productRepository.CreateAsync(product);
            await _productRepository.SaveChangesAsync();

            return product;
        }

        // Update Product
        public async Task<Product> Update(CreateUpdateProductDto input)
        {
            var product = _mapper.Map<Product>(input);

            // Kiểm tra sự tồn tại của sản phẩm với ID cụ thể
            var existingProduct = await _productRepository.GetAsync(product.Id);
            if (existingProduct != null)
            {
                // Cập nhật sản phẩm hiện tại
                existingProduct.ProductName = product.ProductName;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Quantity = product.Quantity;
                existingProduct.Discount = product.Discount;
                existingProduct.SalePrice = product.SalePrice;
                existingProduct.SKU = product.SKU;
                existingProduct.BrandId = product.BrandId;

                // Cập nhật mối quan hệ nhiều-nhiều
                existingProduct.Categories.Clear();
                foreach (var categoryId in input.CategoryIds)
                {
                    var category = await _categoryRepository.GetAsync(categoryId);
                    if (category == null)
                    {
                        throw new ApiException($"Không tìm thấy danh mục với id {categoryId}.");
                    }
                    existingProduct.Categories.Add(category);
                }

                existingProduct.ProductTypes.Clear();
                foreach (var productTypeId in input.ProductTypeIds)
                {
                    var productType = await _productTypeRepository.GetAsync(productTypeId);
                    if (productType == null)
                    {
                        throw new ApiException($"Không tìm thấy loại sản phẩm với id {productTypeId}.");
                    }
                    existingProduct.ProductTypes.Add(productType);
                }

                _productRepository.UpdateAsync(existingProduct);
            }
            else
            {
                // Thêm sản phẩm mới
                await _productRepository.CreateAsync(product);
            }

            await _productRepository.SaveChangesAsync();

            return product;
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
                    var products = await _productRepository.GetAllListAsync();
                    return _mapper.Map<List<ProductDto>>(products);
                }

                public override async Task<ProductDto> Get(Guid id)
                {
                    var product = await _productRepository.AsQueryable().FirstOrDefaultAsync(p => p.Id == id);
                    if (product == null)
                    {
                        throw new ApiException("Product not found.");
                    }

                    return _mapper.Map<ProductDto>(product);
                }

       
            }
        }
