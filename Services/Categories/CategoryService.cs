using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Category;
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
using System.Data;

namespace KhanhSkin_BackEnd.Services.Categories
{
    public class CategoryService : BaseService<Category, CategoryDto, CategoryDto, CategoryGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<ProductType> _productTypeRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            IConfiguration config,
            IRepository<Category> repository,
            IMapper mapper,
            ILogger<CategoryService> logger,
            ICurrentUser currentUser,
            IRepository<ProductType> productTypeRepository) // Inject repository for ProductType
            : base(mapper, repository, logger, currentUser)
        {
            _config = config;
            _categoryRepository = repository;
            _mapper = mapper;
            _logger = logger;
            _productTypeRepository = productTypeRepository; // Assign injected repository
        }

        public async Task<bool> CheckCategoryExist(string categoryName)
        {
            return await _categoryRepository.AsQueryable().AnyAsync(u => u.CategoryName == categoryName);
        }

        public override async Task<Category> Create(CategoryDto input)
        {
            using (var transaction = await _categoryRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted)) // Truyền IsolationLevel
            {
                try
                {
                    // Kiểm tra xem danh mục đã tồn tại chưa
                    if (await CheckCategoryExist(input.CategoryName))
                    {
                        throw new ApiException("Category already exists.");
                    }

                    // Tạo đối tượng Category mới
                    var category = _mapper.Map<Category>(input);

                    // Lấy danh sách ProductTypes từ cơ sở dữ liệu dựa trên ProductTypeIds
                    var productTypes = await _productTypeRepository.AsQueryable()
                        .Where(pt => input.ProductTypeIds.Contains(pt.Id))
                        .ToListAsync();

                    // Kiểm tra xem tất cả ProductTypes có tồn tại không
                    if (productTypes.Count != input.ProductTypeIds.Count)
                    {
                        throw new ApiException("Some ProductTypes not found.");
                    }

                    // Thêm ProductTypes vào Category
                    foreach (var productType in productTypes)
                    {
                        category.ProductTypes.Add(productType);
                    }

                    // Lưu Category mới vào cơ sở dữ liệu
                    await _categoryRepository.CreateAsync(category);
                    await _categoryRepository.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return category;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating category: {CategoryName}", input.CategoryName);
                    throw;
                }
            }
        }

        public override async Task<Category> Update(Guid id, CategoryDto input)
        {
            using (var transaction = await _categoryRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted)) // Truyền IsolationLevel
            {
                try
                {
                    // Find the category in the database
                    var category = await _categoryRepository.AsQueryable()
                        .Include(c => c.ProductTypes)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (category == null)
                    {
                        throw new ApiException("Category not found");
                    }

                    // Check if the category name already exists (if it's changed)
                    if (!string.Equals(category.CategoryName, input.CategoryName, StringComparison.OrdinalIgnoreCase)
                        && await CheckCategoryExist(input.CategoryName))
                    {
                        throw new ApiException("Danh mục đã được sử dụng.");
                    }

                    // Update category properties
                    _mapper.Map(input, category);

                    // Update associated ProductTypes
                    var productTypes = await _productTypeRepository.AsQueryable()
                        .Where(pt => input.ProductTypeIds.Contains(pt.Id))
                        .ToListAsync();

                    if (productTypes.Count != input.ProductTypeIds.Count)
                    {
                        throw new ApiException("Some ProductTypes not found.");
                    }

                    // Clear existing ProductTypes and add the new ones
                    category.ProductTypes.Clear();
                    foreach (var productType in productTypes)
                    {
                        category.ProductTypes.Add(productType);
                    }

                    // Save changes to the database
                    await _categoryRepository.UpdateAsync(category);
                    await _categoryRepository.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Map the updated category to DTO and return
                    return category;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                    throw;
                }
            }
        }

        public async Task<CategoryDto> UpdateCategoryProductTypes(Guid categoryId, List<Guid> productTypeIds)
        {
            using (var transaction = await _categoryRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted)) // Truyền IsolationLevel
            {
                try
                {
                    var category = await _categoryRepository.AsQueryable()
                        .Include(c => c.ProductTypes)
                        .FirstOrDefaultAsync(c => c.Id == categoryId);

                    if (category == null)
                    {
                        throw new ApiException("Category not found.");
                    }

                    var productTypes = await _productTypeRepository.AsQueryable()
                        .Where(pt => productTypeIds.Contains(pt.Id))
                        .ToListAsync();

                    if (productTypes.Count != productTypeIds.Count)
                    {
                        throw new ApiException("Some ProductTypes not found.");
                    }

                    category.ProductTypes.Clear();

                    // Thêm từng ProductType vào danh sách của Category
                    foreach (var productType in productTypes)
                    {
                        if (!category.ProductTypes.Contains(productType))
                        {
                            category.ProductTypes.Add(productType);
                        }
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _categoryRepository.UpdateAsync(category);
                    await _categoryRepository.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Chuyển đổi Category đã cập nhật thành CategoryDto để trả về
                    var updatedCategory = _mapper.Map<CategoryDto>(category);
                    return updatedCategory;
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating category product types: {CategoryId}", categoryId);
                    throw new ApiException("An error occurred while saving the entity changes. See the inner exception for details.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
                }
            }
        }

        public override async Task<List<CategoryDto>> GetAll()
        {
            var categories = await _categoryRepository.AsQueryable()
                .AsNoTracking() // Sử dụng AsNoTracking để tăng hiệu suất
                .Include(c => c.ProductTypes) // Sử dụng Include để tải ProductTypes
                .ToListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<List<CategoryDto>> GetAll(int pageNumber, int pageSize)
        {
            var categories = await _categoryRepository.AsQueryable()
                .AsNoTracking() // Sử dụng AsNoTracking để tăng hiệu suất
                .Include(c => c.ProductTypes) // Sử dụng Include để tải ProductTypes
                .Skip((pageNumber - 1) * pageSize) // Phân trang
                .Take(pageSize)
                .ToListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public override async Task<CategoryDto> Get(Guid id)
        {
            var category = await _categoryRepository.AsQueryable()
                .AsNoTracking() // Sử dụng AsNoTracking để tăng hiệu suất
                .Include(c => c.ProductTypes) // Sử dụng Include để tải ProductTypes
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                throw new ApiException("Category not found.");
            }
            return _mapper.Map<CategoryDto>(category);
        }

        public override async Task<Category> Delete(Guid id)
        {
            using (var transaction = await _categoryRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted)) // Truyền IsolationLevel
            {
                try
                {
                    var category = await _categoryRepository.AsQueryable().FirstOrDefaultAsync(a => a.Id == id);
                    if (category == null)
                    {
                        throw new ApiException("Category not found.");
                    }

                    await _categoryRepository.DeleteAsync(id);
                    await _categoryRepository.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return category; // Return the deleted Category entity
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                    throw;
                }
            }
        }
    }
}
