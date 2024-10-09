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
        private readonly CloudinaryService _cloudinaryService;

        public ProductService(
            IConfiguration config,
            IRepository<Product> repository,
            IRepository<Category> categoryRepository,
            IRepository<ProductType> productTypeRepository,
            IRepository<Brand> brandRepository,
            IRepository<ProductVariant> productVariantRepository,
            IMapper mapper,
            ILogger<ProductService> logger,
            CloudinaryService cloudinaryService,
            ICurrentUser currentUser)
            : base(mapper, repository, logger, currentUser)
        {
            _config = config;
            _productRepository = repository;
            _categoryRepository = categoryRepository;
            _productTypeRepository = productTypeRepository;
            _brandRepository = brandRepository;
            _productVariantRepository = productVariantRepository;
            _cloudinaryService = cloudinaryService;
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





        //public override async Task<Product> Update(Guid id, CreateUpdateProductDto input)
        //{
        //    try
        //    {
        //        Tìm sản phẩm theo ID
        //       var product = await _productRepository.AsQueryable()
        //           .Include(p => p.Variants)
        //           .Include(p => p.Categories)
        //           .Include(p => p.ProductTypes)
        //           .FirstOrDefaultAsync(a => a.Id == id);

        //        if (product == null)
        //        {
        //            _logger.LogError("Product with id {Id} not found", id);
        //            throw new KeyNotFoundException($"Product with id {id} not found");
        //        }

        //        Kiểm tra SKU trùng lặp
        //       await CheckDuplicateSKU(input.SKU, id);

        //        Kiểm tra sự tồn tại của Brand
        //       await CheckBrandExist(input.BrandId);
        //        product.BrandId = input.BrandId;

        //        Cập nhật các trường cơ bản của sản phẩm
        //        _mapper.Map(input, product);

        //        Tính toán giá bán sau khi giảm giá nếu có
        //        CalculateSalePrice(product);

        //        Cập nhật các quan hệ nhiều-nhiều(Categories, ProductTypes)
        //        await UpdateProductCategories(product, input.CategoryIds.ToList());
        //        await UpdateProductTypes(product, input.ProductTypeIds.ToList());
        //        await UpdateProductImages(product, input.Images.ToList());


        //        Cập nhật biến thể sản phẩm
        //        await UpdateProductVariants(product, input.Variants);

        //        Cập nhật sản phẩm trong cơ sở dữ liệu
        //       await _productRepository.UpdateAsync(product);

        //        return product;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating product: {ProductName}", typeof(Product).Name);
        //        throw;
        //    }
        //}
        //private async Task CheckDuplicateSKU(string sku, Guid productId)
        //{
        //    if (await _productRepository.AsQueryable().AnyAsync(p => p.SKU == sku && p.Id != productId))
        //    {
        //        throw new ApiException("SKU đã tồn tại.");
        //    }
        //}
        //private async Task CheckBrandExist(Guid brandId)
        //{
        //    var brand = await _brandRepository.GetAsync(brandId);
        //    if (brand == null)
        //    {
        //        throw new ApiException($"Không tìm thấy thương hiệu với id {brandId}.");
        //    }
        //}
        //private void CalculateSalePrice(Product product)
        //{
        //    if (product.Discount.HasValue && product.Discount.Value > 0 && product.Discount.Value <= 100)
        //    {
        //        product.SalePrice = product.Price - (product.Price * product.Discount.Value / 100);
        //    }
        //    else
        //    {
        //        product.SalePrice = product.Price;
        //    }
        //}
        //private async Task UpdateProductCategories(Product product, List<Guid> categoryIds)
        //{
        //    var existingCategoryIds = product.Categories.Select(c => c.Id).ToList();
        //    var newCategoryIds = categoryIds.Except(existingCategoryIds).ToList();
        //    var removedCategoryIds = existingCategoryIds.Except(categoryIds).ToList();

        //    foreach (var categoryId in removedCategoryIds)
        //    {
        //        var categoryToRemove = product.Categories.FirstOrDefault(c => c.Id == categoryId);
        //        if (categoryToRemove != null)
        //        {
        //            product.Categories.Remove(categoryToRemove);
        //        }
        //    }

        //    foreach (var categoryId in newCategoryIds)
        //    {
        //        var category = await _categoryRepository.GetAsync(categoryId);
        //        if (category == null)
        //        {
        //            throw new ApiException($"Không tìm thấy danh mục với id {categoryId}.");
        //        }
        //        product.Categories.Add(category);
        //    }
        //}

        //private async Task UpdateProductTypes(Product product, List<Guid> productTypeIds)
        //{
        //    var existingProductTypeIds = product.ProductTypes.Select(pt => pt.Id).ToList();
        //    var newProductTypeIds = productTypeIds.Except(existingProductTypeIds).ToList();
        //    var removedProductTypeIds = existingProductTypeIds.Except(productTypeIds).ToList();

        //    foreach (var productTypeId in removedProductTypeIds)
        //    {
        //        var productTypeToRemove = product.ProductTypes.FirstOrDefault(pt => pt.Id == productTypeId);
        //        if (productTypeToRemove != null)
        //        {
        //            product.ProductTypes.Remove(productTypeToRemove);
        //        }
        //    }

        //    foreach (var productTypeId in newProductTypeIds)
        //    {
        //        var productType = await _productTypeRepository.GetAsync(productTypeId);
        //        if (productType == null)
        //        {
        //            throw new ApiException($"Không tìm thấy loại sản phẩm với id {productTypeId}.");
        //        }
        //        product.ProductTypes.Add(productType);
        //    }
        //}

        //private async Task UpdateProductImages(Product product, List<string> imageUrls)
        //{
        //    product.Images = imageUrls; // Cập nhật danh sách ảnh sản phẩm với các URL mới
        //}


        //private async Task UpdateProductVariants(Product product, List<ProductVariantDto> variantsDto)
        //{
        //    var existingVariants = await _productVariantRepository.AsQueryable().Where(v => v.ProductId == product.Id).ToListAsync();
        //    _productVariantRepository.Table.RemoveRange(existingVariants); // Xóa các biến thể hiện tại
        //    await _productVariantRepository.SaveChangesAsync();

        //    var variants = new List<ProductVariant>();
        //    if (variantsDto != null && variantsDto.Any())
        //    {
        //        var totalVariantQuantity = variantsDto.Sum(v => v.QuantityVariant);
        //        if (totalVariantQuantity != product.Quantity)
        //        {
        //            throw new ApiException("Tổng số lượng biến thể phải bằng với Quantity của sản phẩm.");
        //        }

        //        var variantSKUs = new HashSet<string>();
        //        foreach (var variantDto in variantsDto)
        //        {
        //            if (variantSKUs.Contains(variantDto.SKUVariant))
        //            {
        //                throw new ApiException("Các biến thể trong danh sách đầu vào không được có SKU trùng lặp.");
        //            }
        //            variantSKUs.Add(variantDto.SKUVariant);

        //            if (await CheckVariantExist(variantDto.NameVariant, variantDto.SKUVariant, product.Id))
        //            {
        //                throw new ApiException("Biến thể đã tồn tại cho sản phẩm này hoặc SKU biến thể đã tồn tại.");
        //            }

        //            var variant = _mapper.Map<ProductVariant>(variantDto);
        //            CalculateVariantSalePrice(variant);

        //            variant.Id = Guid.NewGuid();
        //            variants.Add(variant);
        //        }
        //    }

        //    foreach (var variant in variants)
        //    {
        //        variant.ProductId = product.Id;
        //        await _productVariantRepository.CreateAsync(variant);
        //    }

        //    await _productVariantRepository.SaveChangesAsync();
        //}


        //private void CalculateVariantSalePrice(ProductVariant variant)
        //{
        //    if (variant.DiscountVariant.HasValue && variant.DiscountVariant.Value > 0 && variant.DiscountVariant.Value <= 100)
        //    {
        //        variant.SalePriceVariant = variant.PriceVariant - (variant.PriceVariant * variant.DiscountVariant.Value / 100);
        //    }
        //    else
        //    {
        //        variant.SalePriceVariant = variant.PriceVariant;
        //    }
        //}
        //private async Task<bool> CheckVariantExist(string nameVariant, string skuVariant, Guid productId)
        //{
        //    return await _productVariantRepository.AsQueryable()
        //        .AnyAsync(v => (v.NameVariant == nameVariant || v.SKUVariant == skuVariant) && v.ProductId == productId);
        //}





        public override async Task<Product> Update(Guid id, CreateUpdateProductDto input)
        {
            try
            {
                // Tìm sản phẩm theo ID
                var product = await _productRepository.AsQueryable()
                    .Include(p => p.Variants)
                    .Include(p => p.Categories)
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

                // Kiểm tra sự tồn tại của Brand dựa trên BrandId
                var brand = await _brandRepository.GetAsync(input.BrandId);
                if (brand == null)
                {
                    throw new ApiException($"Không tìm thấy thương hiệu với id {input.BrandId}.");
                }
                product.BrandId = input.BrandId;

                // Cập nhật thông tin sản phẩm từ DTO
                _mapper.Map(input, product);

                // Quản lý ảnh sản phẩm: giữ lại ảnh cũ và thêm ảnh mới
                var existingImages = product.Images ?? new List<string>();
                var newImages = input.Images.Except(existingImages).ToList(); // Chỉ thêm ảnh mới
                product.Images = existingImages.Union(newImages).ToList(); // Gộp ảnh cũ và ảnh mới

                // Xóa ảnh bị xóa từ FE
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

                // Cập nhật mối quan hệ nhiều-nhiều cho Categories
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

                // Cập nhật mối quan hệ nhiều-nhiều cho ProductTypes
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

                // Cập nhật biến thể sản phẩm: tương tự như sản phẩm, giữ ảnh cũ và thêm ảnh mới
                var existingVariants = await _productVariantRepository.AsQueryable().Where(v => v.ProductId == id).ToListAsync();
                var variantSKUs = new HashSet<string>();
                foreach (var existingVariant in existingVariants)
                {
                    variantSKUs.Add(existingVariant.SKUVariant);
                }

                var variantsToUpdate = new List<ProductVariant>();
                if (input.Variants != null && input.Variants.Any())
                {
                    var totalVariantQuantity = input.Variants.Sum(v => v.QuantityVariant);

                    if (totalVariantQuantity != input.Quantity)
                    {
                        throw new ApiException("Tổng số lượng biến thể phải bằng với Quantity của sản phẩm.");
                    }

                    foreach (var variantDto in input.Variants)
                    {
                        // Giữ lại ảnh cũ của biến thể và thêm ảnh mới
                        var existingVariant = existingVariants.FirstOrDefault(v => v.SKUVariant == variantDto.SKUVariant);
                        var newVariant = _mapper.Map<ProductVariant>(variantDto);

                        var existingVariantImage = existingVariant?.ImageUrl;
                        var newVariantImage = variantDto.ImageUrl;

                        // Chỉ xóa ảnh cũ nếu có ảnh mới và ảnh cũ khác với ảnh mới
                        if (!string.IsNullOrEmpty(existingVariantImage) && existingVariantImage != newVariantImage)
                        {
                            var publicId = ExtractPublicIdFromUrl(existingVariantImage);
                            if (publicId != null)
                            {
                                await _cloudinaryService.DeleteImageAsync(publicId); // Xóa ảnh cũ
                            }
                        }

                        newVariant.ImageUrl = newVariantImage;
                        variantsToUpdate.Add(newVariant);
                    }
                }

                // Xóa các biến thể cũ và thêm các biến thể mới
                _productVariantRepository.Table.RemoveRange(existingVariants);
                await _productVariantRepository.SaveChangesAsync();

                foreach (var variant in variantsToUpdate)
                {
                    variant.ProductId = product.Id;
                    await _productVariantRepository.CreateAsync(variant);
                }

                await _productVariantRepository.SaveChangesAsync();

                // Cập nhật sản phẩm trong cơ sở dữ liệu
                await _productRepository.UpdateAsync(product);

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductName}", typeof(Product).Name);
                throw;
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
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new ApiException("Product not found.");
            }

            return _mapper.Map<ProductDto>(product);
        }



        public virtual IQueryable<ProductDto> CreateFilteredQuery(ProductGetRequestInputDto input)
        {
            // Bắt đầu với một query cơ bản từ repository
            var query = _repository.AsQueryable();

            // Áp dụng các điều kiện lọc nếu có
            if (input.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == input.BrandId.Value);
            }

            if (input.CategoryIds != null && input.CategoryIds.Any())
            {
                query = query.Where(p => p.Categories.Any(c => input.CategoryIds.Contains(c.Id)));
            }

            if (input.ProductTypeIds != null && input.ProductTypeIds.Any())
            {
                query = query.Where(p => p.ProductTypes.Any(pt => input.ProductTypeIds.Contains(pt.Id)));
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
    

    //public override IQueryable<Product> CreateFilteredQuery(ProductGetRequestInputDto input)
    //{
    //    // Tạo một truy vấn cơ bản từ phương thức cơ sở
    //    var query = base.CreateFilteredQuery(input);

    //    // Kiểm tra nếu có BrandId để lọc các sản phẩm theo thương hiệu
    //    if (input.BrandId.HasValue)
    //    {
    //        query = query.Where(p => p.BrandId == input.BrandId.Value);
    //    }

    //    // Kiểm tra nếu có CategoryIds để lọc các sản phẩm theo danh mục
    //    if (input.CategoryIds != null && input.CategoryIds.Any())
    //    {
    //        query = query.Where(p => p.Categories.Any(c => input.CategoryIds.Contains(c.Id)));
    //    }

    //    // Kiểm tra nếu có ProductTypeIds để lọc các sản phẩm theo loại sản phẩm
    //    if (input.ProductTypeIds != null && input.ProductTypeIds.Any())
    //    {
    //        query = query.Where(p => p.ProductTypes.Any(pt => input.ProductTypeIds.Contains(pt.Id)));
    //    }

    //    // Áp dụng phân trang và sắp xếp nếu có
    //    if (!string.IsNullOrWhiteSpace(input.Sort))
    //    {
    //        query = query.OrderBy(input.Sort);
    //    }

    //    if (input.PageIndex.HasValue && input.PageSize.HasValue)
    //    {
    //        query = query.Skip((input.PageIndex.Value - 1) * input.PageSize.Value).Take(input.PageSize.Value);
    //    }

    //    return query; // Trả về truy vấn đã được lọc
    //}



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