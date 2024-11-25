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
using System.Text.RegularExpressions;
using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Dtos.Category;
using KhanhSkin_BackEnd.Dtos.ProductType;
using KhanhSkin_BackEnd.Dtos.Cart;
using KhanhSkin_BackEnd.Services.Carts;
using KhanhSkin_BackEnd.Share.Dtos;
using KhanhSkin_BackEnd.Dtos.Review;
using KhanhSkin_BackEnd.Consts;


namespace KhanhSkin_BackEnd.Services.Products
{
    public class ProductService : BaseService<Product, ProductDto, CreateUpdateProductDto, ProductGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<KhanhSkin_BackEnd.Entities.InventoryLog> _inventoryLogRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductType> _productTypeRepository;
        private readonly IRepository<Brand> _brandRepository;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IRepository<CartItem> _cartItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ProductRecommenService _productRecommenService;
        private readonly CartService _cartService;


        public ProductService(
            IConfiguration config,
            IRepository<Product> repository,
            IRepository<Category> categoryRepository,
            IRepository<Supplier> supplierRepository,
            IRepository<ProductType> productTypeRepository,
            IRepository<Brand> brandRepository,
            IRepository<ProductVariant> productVariantRepository,
            IRepository<CartItem> cartItemRepository,
            IRepository<KhanhSkin_BackEnd.Entities.InventoryLog> inventoryLogRepository,
            IMapper mapper,
            ILogger<ProductService> logger,
            CloudinaryService cloudinaryService,
            ProductRecommenService productRecommenService,
            CartService cartService,
            ICurrentUser currentUser)
            : base(mapper, repository, logger, currentUser)
        {
            _config = config;
            _productRepository = repository;
            _productRecommenService = productRecommenService;
            _inventoryLogRepository = inventoryLogRepository;
            _supplierRepository = supplierRepository;
            _categoryRepository = categoryRepository;
            _productTypeRepository = productTypeRepository;
            _brandRepository = brandRepository;
            _productVariantRepository = productVariantRepository;
            _cartItemRepository = cartItemRepository;
            _cloudinaryService = cloudinaryService;
            _cartService = cartService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> CheckProductExist(string productName)
        {
            return await _productRepository.AsQueryable().AnyAsync(u => u.ProductName == productName);
        }

        public async Task UpdateProductAverageRating(Guid productId)
        {
            var product = await _productRepository.AsQueryable()
                .Include(p => p.Reviews) // Bao gồm tất cả các đánh giá
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                _logger.LogWarning($"Product with ID {productId} not found. Cannot update average rating.");
                return;
            }

            // Chuyển các đánh giá sang dạng ReviewDataDto
            var allReviews = product.Reviews?.Select(r => new ReviewDataDto
            {
                Rating = r.Rating,
                Comment = r.Comment,
                IsApproved = r.IsApproved
            }).ToList() ?? new List<ReviewDataDto>();

            // Tính toán TotalRating và ReviewCount dựa trên tất cả các đánh giá
            product.TotalRating = allReviews.Sum(r => r.Rating);
            product.ReviewCount = allReviews.Count;

            // Tính toán AverageRating dựa trên tất cả các đánh giá
            product.AverageRating = product.ReviewCount > 0
                ? (decimal)product.TotalRating / product.ReviewCount
                : 0;

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation($"Updated Product: {productId}, TotalRating: {product.TotalRating}, ReviewCount: {product.ReviewCount}, AverageRating: {product.AverageRating}");
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
                    .Include(p => p.Variants)
                    .Include(p => p.Categories)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductTypes)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (product == null)
                {
                    _logger.LogError("Product with id {Id} not found", id);
                    throw new KeyNotFoundException($"Product with id {id} not found");
                }

                // Kiểm tra SKU có tồn tại nhưng không phải của sản phẩm hiện tại
                if (await _productRepository.AsQueryable().AnyAsync(p => p.SKU == input.SKU && p.Id != id))
                {
                    throw new ApiException("SKU đã tồn tại.");
                }

                // Kiểm tra sự tồn tại của Brand
                var brand = await _brandRepository.GetAsync(input.BrandId);
                if (brand == null)
                {
                    throw new ApiException($"Không tìm thấy thương hiệu với id {input.BrandId}.");
                }
                product.BrandId = input.BrandId;

                // Cập nhật thông tin sản phẩm từ DTO
                _mapper.Map(input, product);

                // Xử lý hình ảnh sản phẩm
                var existingImages = product.Images ?? new List<string>();
                var newImages = input.Images.Except(existingImages).ToList();
                product.Images = existingImages.Union(newImages).ToList();

                var removedImages = existingImages.Except(input.Images).ToList();
                foreach (var image in removedImages)
                {
                    var publicId = ExtractPublicIdFromUrl(image);
                    if (publicId != null)
                    {
                        await _cloudinaryService.DeleteImageAsync(publicId);
                    }
                }

                // Tính toán SalePrice nếu có Discount
                if (product.Discount.HasValue && product.Discount.Value > 0 && product.Discount.Value <= 100)
                {
                    product.SalePrice = product.Price - (product.Price * product.Discount.Value / 100);
                }
                else
                {
                    product.SalePrice = product.Price;
                }

                // Tìm tất cả CartItems liên quan đến sản phẩm
                var relatedCartItems = await _cartItemRepository.AsQueryable()
                    .Where(ci => ci.ProductId == id)
                    .ToListAsync();

                // **Cập nhật thông tin sản phẩm trong CartItem**
                foreach (var cartItem in relatedCartItems)
                {
                    if (cartItem.VariantId == null)
                    {
                        // Cập nhật thông tin cho sản phẩm không có biến thể
                        cartItem.ProductName = product.ProductName;
                        cartItem.ProductPrice = product.Price;
                        cartItem.ProductSalePrice = (decimal)product.SalePrice;
                        cartItem.Images = product.Images; // Cập nhật danh sách hình ảnh
                    }
                    else
                    {
                        // Cập nhật thông tin cho sản phẩm có biến thể
                        var variant = product.Variants.FirstOrDefault(v => v.Id == cartItem.VariantId);
                        if (variant != null)
                        {
                            cartItem.ProductName = product.ProductName;
                            cartItem.NameVariant = variant.NameVariant; // Tên biến thể
                            cartItem.ProductPrice = variant.PriceVariant; // Giá của biến thể
                            cartItem.ProductSalePrice = (decimal)variant.SalePriceVariant;
                        }
                    }
                }

                // Lưu tất cả các thay đổi của CartItem sau khi cập nhật
                await _cartItemRepository.SaveChangesAsync();

                // Cập nhật hoặc thêm mới các biến thể
                var existingVariants = await _productVariantRepository.AsQueryable()
                    .Where(v => v.ProductId == id)
                    .ToListAsync();

                foreach (var variantDto in input.Variants)
                {
                    var existingVariant = existingVariants.FirstOrDefault(v => v.SKUVariant == variantDto.SKUVariant);

                    if (existingVariant != null)
                    {
                        // Cập nhật biến thể hiện có
                        _mapper.Map(variantDto, existingVariant);
                        existingVariant.ProductId = product.Id; // Đảm bảo ProductId được gán đúng
                    }
                    else
                    {
                        // Thêm biến thể mới
                        var newVariant = _mapper.Map<ProductVariant>(variantDto);
                        newVariant.ProductId = product.Id; // Gán ProductId cho biến thể mới
                        await _productVariantRepository.CreateAsync(newVariant);
                    }
                }

                // Xóa các biến thể không còn tồn tại trong danh sách input
                var variantsToRemove = existingVariants
                    .Where(v => !input.Variants.Any(dto => dto.SKUVariant == v.SKUVariant))
                    .ToList();

                if (variantsToRemove.Any())
                {
                    _productVariantRepository.Table.RemoveRange(variantsToRemove);
                    await _productVariantRepository.SaveChangesAsync();
                }

                // Cập nhật các quan hệ nhiều-nhiều khác (Categories, ProductTypes)
                await UpdateProductRelationships(product, input);

                // Lưu thay đổi sản phẩm
                await _productRepository.UpdateAsync(product);

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductName}", typeof(Product).Name);
                throw;
            }
        }  


        // Hàm phụ trợ để cập nhật quan hệ nhiều-nhiều (Categories, ProductTypes)
        private async Task UpdateProductRelationships(Product product, CreateUpdateProductDto input)
        {
            // Cập nhật quan hệ Categories
            var existingCategoryIds = product.Categories.Select(c => c.Id).ToList();
            var newCategoryIds = input.CategoryIds.Except(existingCategoryIds).ToList();
            var removedCategoryIds = existingCategoryIds.Except(input.CategoryIds).ToList();

            foreach (var categoryId in removedCategoryIds)
            {
                var categoryToRemove = product.Categories.FirstOrDefault(c => c.Id == categoryId);
                if (categoryToRemove != null)
                {
                    product.Categories.Remove(categoryToRemove);
                }
            }

            foreach (var categoryId in newCategoryIds)
            {
                var category = await _categoryRepository.GetAsync(categoryId);
                if (category == null)
                {
                    throw new ApiException($"Không tìm thấy danh mục với id {categoryId}.");
                }
                product.Categories.Add(category);
            }

            // Cập nhật quan hệ ProductTypes
            var existingProductTypeIds = product.ProductTypes.Select(pt => pt.Id).ToList();
            var newProductTypeIds = input.ProductTypeIds.Except(existingProductTypeIds).ToList();
            var removedProductTypeIds = existingProductTypeIds.Except(input.ProductTypeIds).ToList();

            foreach (var productTypeId in removedProductTypeIds)
            {
                var productTypeToRemove = product.ProductTypes.FirstOrDefault(pt => pt.Id == productTypeId);
                if (productTypeToRemove != null)
                {
                    product.ProductTypes.Remove(productTypeToRemove);
                }
            }

            foreach (var productTypeId in newProductTypeIds)
            {
                var productType = await _productTypeRepository.GetAsync(productTypeId);
                if (productType == null)
                {
                    throw new ApiException($"Không tìm thấy loại sản phẩm với id {productTypeId}.");
                }
                product.ProductTypes.Add(productType);
            }
        }



        public override async Task<Product> Delete(Guid id)
        {
            // Lấy thông tin sản phẩm bao gồm cả các biến thể
            var product = await _productRepository.AsQueryable()
                                   .Include(p => p.Variants)
                                   .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new ApiException("Product not found.");
            }

            // Xóa các CartItems liên quan đến sản phẩm và biến thể của sản phẩm
            var cartItems = await _cartItemRepository.AsQueryable()
                              .Where(ci => ci.ProductId == id || ci.Variant.ProductId == id)
                              .ToListAsync();

            _cartItemRepository.Table.RemoveRange(cartItems);
            await _cartItemRepository.SaveChangesAsync();



            // Xóa ảnh chính của sản phẩm từ Cloudinary
            foreach (var image in product.Images)
            {
                var publicId = ExtractPublicIdFromUrl(image); // Cần có phương thức để trích xuất publicId
                if (publicId != null)
                {
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
            }

            // Xóa ảnh của các biến thể sản phẩm từ Cloudinary
            foreach (var variant in product.Variants)
            {
                var variantImageUrl = variant.ImageUrl;
                var variantPublicId = ExtractPublicIdFromUrl(variantImageUrl); // Cần có phương thức để trích xuất publicId
                if (variantPublicId != null)
                {
                    await _cloudinaryService.DeleteImageAsync(variantPublicId);
                }
            }

            // Xóa sản phẩm (các biến thể sẽ tự động bị xóa do cascade)
            await _productRepository.DeleteAsync(id);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _productRepository.SaveChangesAsync();

            // Trả về đối tượng product đã bị xóa
            return product;
        }

        // Phương thức để trích xuất public ID từ URL Cloudinary
        private string ExtractPublicIdFromUrl(string url)
        {
            var match = Regex.Match(url, @"/v\d+/(?<publicId>.+)\.[a-zA-Z]+$");
            return match.Success ? match.Groups["publicId"].Value : null;
        }



        public override async Task<List<ProductDto>> GetAll()
        {
            var products = await _productRepository.AsQueryable()
                .Include(p => p.Brand)
                .Include(p => p.Categories)
                .Include(p => p.ProductTypes)
                .Include(p => p.Variants)
                .OrderByDescending(p => p.CreatedDate)
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
               .OrderByDescending(p => p.CreatedDate)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new ApiException("Product not found.");
            }

            return _mapper.Map<ProductDto>(product);
        }

        public virtual async Task<PagedViewModel<ProductDto>> GetPagedProducts(ProductGetRequestInputDto input)
        {
            // Bắt đầu từ truy vấn cơ bản
            var query = CreateFilteAllProduct(input);

            // Đếm tổng số bản ghi thỏa mãn điều kiện
            var totalCount = await query.CountAsync();

            // Áp dụng phân trang
            query = query.Skip((input.PageIndex - 1) * input.PageSize)
                         .Take(input.PageSize);

            // Lấy dữ liệu sau khi phân trang
            var data = await query.ToListAsync();

            // Trả về kết quả dưới dạng `PagedViewModel`
            return new PagedViewModel<ProductDto>
            {
                Items = data,
                TotalRecord = totalCount
            };
        }



        public virtual IQueryable<ProductDto> CreateFilteAllProduct(ProductGetRequestInputDto input)
        {
            // Bắt đầu với một query cơ bản từ repository
            var query = _repository.AsQueryable();

            // Tìm kiếm theo FreeTextSearch (ProductName hoặc SKU)
            if (!string.IsNullOrEmpty(input.FreeTextSearch))
            {
                var freeTextSearch = input.FreeTextSearch.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(freeTextSearch) ||
                                          p.SKU.ToLower().Contains(freeTextSearch));
            }

            // Áp dụng sắp xếp dựa trên SortBy và IsAscending
            if (!string.IsNullOrWhiteSpace(input.SortBy))
            {
                // Xác định chiều sắp xếp
                var sortingOrder = input.IsAscending ? "ascending" : "descending";

                // Sắp xếp theo các tiêu chí khác nhau dựa trên SortBy
                switch (input.SortBy.ToLower())
                {
                    case "price":
                        query = input.IsAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                        break;

                    case "purchases":
                        query = input.IsAscending ? query.OrderBy(p => p.Purchases) : query.OrderByDescending(p => p.Purchases);
                        break;

                    case "averagerating":
                        query = input.IsAscending ? query.OrderBy(p => p.AverageRating) : query.OrderByDescending(p => p.AverageRating);
                        break;

                    case "quantity":
                        query = input.IsAscending ? query.OrderBy(p => p.Quantity) : query.OrderByDescending(p => p.Quantity);
                        break;


                    case "createddate":
                    default:
                        // Mặc định sắp xếp theo ngày tạo (mới nhất)
                        query = input.IsAscending ? query.OrderBy(p => p.CreatedDate) : query.OrderByDescending(p => p.CreatedDate);
                        break;
                }
            }
            else
            {
                // Mặc định sắp xếp theo ngày tạo nếu không có điều kiện sắp xếp
                query = query.OrderByDescending(p => p.CreatedDate);
            }

            // Lựa chọn các trường cần thiết
            var selectedQuery = query
                .Include(p => p.Brand)        // Bao gồm các bảng liên quan nếu cần
                .Include(p => p.Categories)   // Bao gồm danh mục sản phẩm
                .Include(p => p.ProductTypes) // Bao gồm loại sản phẩm
                .Include(p => p.Variants)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Discount = p.Discount,
                    SalePrice = p.SalePrice,
                    Brand = new BrandDto
                    {
                        Id = p.Brand.Id,
                        BrandName = p.Brand.BrandName
                    },
                    Categories = p.Categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        CategoryName = c.CategoryName
                    }).ToList(),
                    ProductTypes = p.ProductTypes.Select(pt => new ProductTypeDto
                    {
                        Id = pt.Id,
                        TypeName = pt.TypeName
                    }).ToList(),
                    Variants = p.Variants.Select(v => new ProductVariantDto
                    {
                        Id = v.Id,// Lấy ra NameVarian
                        NameVariant = v.NameVariant,
                        SKUVariant = v.SKUVariant,
                        PriceVariant = v.PriceVariant,
                        SalePriceVariant = v.SalePriceVariant,
                        QuantityVariant = v.QuantityVariant,
                        ImageUrl = v.ImageUrl,

                    }).ToList(),

                    Purchases = p.Purchases,
                    AverageRating = p.AverageRating,
                    Images = p.Images,
                    Quantity = p.Quantity,
                    SKU = p.SKU,
                    Description = p.Description,

                });

            return selectedQuery;
        }

        /// //////////////
      
        public virtual IQueryable<ProductDto> CreateFilteredQuery(ProductGetRequestInputDto input)
        {
            // Bắt đầu với một query cơ bản từ repository
            var query = _repository.AsQueryable();
            // Tìm kiếm theo FreeTextSearch (ProductName hoặc SKU)
            if (!string.IsNullOrEmpty(input.FreeTextSearch))
            {
                var freeTextSearch = input.FreeTextSearch.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(freeTextSearch) ||
                                          p.SKU.ToLower().Contains(freeTextSearch));
            }


            // Áp dụng các điều kiện lọc nếu có
            // Áp dụng các điều kiện lọc nếu có
            if (input.BrandIds != null && input.BrandIds.Any())
            {
                query = query.Where(p => input.BrandIds.Contains(p.BrandId));
            }

            if (input.CategoryIds != null && input.CategoryIds.Any())
            {
                query = query.Where(p => p.Categories.Any(c => input.CategoryIds.Contains(c.Id)));
            }

            if (input.ProductTypeIds != null && input.ProductTypeIds.Any())
            {
                query = query.Where(p => p.ProductTypes.Any(pt => input.ProductTypeIds.Contains(pt.Id)));
            }

            // Lọc theo MinPrice và MaxPrice nếu có
            if (input.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= input.MinPrice.Value);
            }

            if (input.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= input.MaxPrice.Value);
            }

           


            // Áp dụng sắp xếp dựa trên SortBy và IsAscending
            if (!string.IsNullOrWhiteSpace(input.SortBy))
            {
                // Xác định chiều sắp xếp
                var sortingOrder = input.IsAscending ? "ascending" : "descending";

                // Sắp xếp theo các tiêu chí khác nhau dựa trên SortBy
                switch (input.SortBy.ToLower())
                {
                    case "price":
                        query = input.IsAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                        break;

                    case "purchases":
                        query = input.IsAscending ? query.OrderBy(p => p.Purchases) : query.OrderByDescending(p => p.Purchases);
                        break;

                    case "averagerating":
                        query = input.IsAscending ? query.OrderBy(p => p.AverageRating) : query.OrderByDescending(p => p.AverageRating);
                        break;

                    case "quantity":
                        query = input.IsAscending ? query.OrderBy(p => p.Quantity) : query.OrderByDescending(p => p.Quantity);
                        break;


                    case "createddate":
                    default:
                        // Mặc định sắp xếp theo ngày tạo (mới nhất)
                        query = input.IsAscending ? query.OrderBy(p => p.CreatedDate) : query.OrderByDescending(p => p.CreatedDate);
                        break;
                }
            }
            else
            {
                // Mặc định sắp xếp theo ngày tạo nếu không có điều kiện sắp xếp
                query = query.OrderByDescending(p => p.CreatedDate);
            }

            // Lựa chọn các trường cần thiết
            var selectedQuery = query
                .Include(p => p.Brand)        // Bao gồm các bảng liên quan nếu cần
                .Include(p => p.Categories)   // Bao gồm danh mục sản phẩm
                .Include(p => p.ProductTypes) // Bao gồm loại sản phẩm

                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Discount = p.Discount,
                    SalePrice = p.SalePrice,
                    Brand = new BrandDto
                    {
                        Id = p.Brand.Id,
                        BrandName = p.Brand.BrandName
                    },
                    Categories = p.Categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        CategoryName = c.CategoryName
                    }).ToList(),
                    ProductTypes = p.ProductTypes.Select(pt => new ProductTypeDto
                    {
                        Id = pt.Id,
                        TypeName = pt.TypeName
                    }).ToList(),
                    Purchases = p.Purchases,
                    AverageRating = p.AverageRating,
                    Images = p.Images
                });

            return selectedQuery;
        }


        // Sử dụng phương thức CreateFilteredQuery
        public async Task<List<ProductDto>> GetFilteredProducts(ProductGetRequestInputDto input)
        {
            var filteredProducts = await CreateFilteredQuery(input)

                .ToListAsync();

            return filteredProducts;
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
                .OrderByDescending(p => p.CreatedDate)
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
                .OrderByDescending(p => p.CreatedDate)
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
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<ProductOutstandingDto>>(products);
        }


        //nhập hàng

 
        private string GenerateCodeInventory()
        {
            // Tạo chuỗi với tiền tố "ORD-", sau đó lấy 4 ký tự ngẫu nhiên từ Guid và ghép vào phần thời gian ngắn gọn
            string datePart = DateTime.UtcNow.ToString("MMdd");
            string randomPart = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();

            // Kết hợp các phần lại thành mã ngắn gọn
            return $"NH-{datePart}{randomPart}";
        }


        public async Task ImportProductInventoryAsync(List<ProductInventoryImportDto> inputs)
        {
            foreach (var input in inputs)
            {
                var product = await _productRepository.AsQueryable()
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == input.ProductId);

                if (product == null)
                {
                    throw new ApiException("Không tìm thấy sản phẩm.");
                }

                decimal itemPrice = input?.CostPrice ?? 0;
                decimal totalPrice = itemPrice * input.Quantity;
                string codeInventory = GenerateCodeInventory();

                string productImage = product.Images?.FirstOrDefault(); // Lấy ảnh đầu tiên của sản phẩm nếu có
                string variantImage = null;

                if (input.ProductVariantId.HasValue)
                {
                    var variant = product.Variants.FirstOrDefault(v => v.Id == input.ProductVariantId.Value);

                    if (variant == null)
                    {
                        throw new ApiException("Không tìm thấy biến thể sản phẩm hoặc biến thể không thuộc về sản phẩm này.");
                    }

                    variant.QuantityVariant += input.Quantity;
                    product.Quantity = product.Variants.Sum(v => v.QuantityVariant);

                    await _productVariantRepository.UpdateAsync(variant);
                    await _productVariantRepository.SaveChangesAsync();

                    variantImage = variant.ImageUrl; // Lấy ảnh của biến thể nếu nhập cho biến thể
                }
                else
                {
                    if (product.Variants != null && product.Variants.Any())
                    {
                        throw new ApiException("Sản phẩm này có biến thể. Vui lòng chỉ định biến thể cụ thể để nhập hàng.");
                    }

                    product.Quantity += input.Quantity;
                }

                await _productRepository.UpdateAsync(product);
                await _productRepository.SaveChangesAsync();

                var inventoryLog = new KhanhSkin_BackEnd.Entities.InventoryLog
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    ProductSKU = product.SKU,
                    QuantityChange = input.Quantity,
                    TransactionDate = DateTime.UtcNow,
                    ActionType = Enums.ActionType.Import,
                    SupplierId = input.SupplierId,
                    SupplierName = input.SupplierId.HasValue ? (await _supplierRepository.GetAsync(input.SupplierId.Value))?.SupplierName : null,
                    CostPrice = input.CostPrice,
                    ItemPrice = itemPrice,
                    TotalPrice = totalPrice,
                    Note = input.Note,
                    CodeInventory = codeInventory,
                    ProductImage = productImage,    // URL ảnh của sản phẩm
                    VariantImage = variantImage     // URL ảnh của biến thể
                };

                if (input.ProductVariantId.HasValue)
                {
                    var variant = product.Variants.FirstOrDefault(v => v.Id == input.ProductVariantId.Value);
                    inventoryLog.ProductVariantId = variant.Id;
                    inventoryLog.VariantName = variant.NameVariant;
                    inventoryLog.VariantSKU = variant.SKUVariant;
                }

                await _inventoryLogRepository.CreateAsync(inventoryLog);
                await _inventoryLogRepository.SaveChangesAsync();
            }
        }




        //
        public async Task<List<ProductOutstandingDto>> RecommendProductsAsync(string productId, int topN)
        {
            var recommendationIds = await _productRecommenService.GetRecommendationsAsync(productId, topN);

            if (recommendationIds == null || !recommendationIds.Any())
            {
                return new List<ProductOutstandingDto>();
            }

            var products = await _productRepository.AsQueryable()
                .Where(p => recommendationIds.Contains(p.Id))
                .Include(p => p.Brand)
                .Include(p => p.Categories)
                .Include(p => p.ProductTypes)
                .Include(p => p.Variants)
                .ToListAsync();

            return _mapper.Map<List<ProductOutstandingDto>>(products);
        }


    }

}

