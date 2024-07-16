using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Category;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

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
        
            // Existing constructor and other methods remain unchanged

            public async Task<CategoryDto> Create(CategoryDto input)
            {
                if (await CheckCategoryExist(input.CategoryName))
                {
                    throw new ApiException("Category already exists.");
                }

                var category = _mapper.Map<Category>(input);

                await _categoryRepository.CreateAsync(category);
                await _categoryRepository.SaveChangesAsync();

                var newCategory = _mapper.Map<CategoryDto>(category);
                return newCategory;
            }

        // Phương thức thêm ProductType vào Category
        public async Task<CategoryDto> AddProductTypes(Guid categoryId, List<Guid> productTypeIds)
        {
            try
            {
                // Tìm Category từ cơ sở dữ liệu
                var category = await _categoryRepository.AsQueryable().FirstOrDefaultAsync(c => c.Id == categoryId);
                if (category == null)
                {
                    throw new ApiException("Category not found.");
                }

                // Tìm danh sách ProductType từ cơ sở dữ liệu
                var productTypes = await _productTypeRepository.AsQueryable().Where(pt => productTypeIds.Contains(pt.Id)).ToListAsync();
                if (productTypes.Count != productTypeIds.Count)
                {
                    throw new ApiException("Some ProductTypes not found.");
                }

                // Thêm từng ProductType vào danh sách của Category
                foreach (var productType in productTypes)
                {
                    if (!category.ProductTypes.Contains(productType))
                    {
                        category.ProductTypes.Add(productType);
                    }
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                await _categoryRepository.SaveChangesAsync();

                // Chuyển đổi Category đã cập nhật thành CategoryDto để trả về
                var updatedCategory = _mapper.Map<CategoryDto>(category);
                return updatedCategory;
            }
            catch (DbUpdateException ex)
            {
                throw new ApiException("An error occurred while saving the entity changes. See the inner exception for details.");
            }
            catch (Exception ex)
            {
                throw new ApiException($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        public async Task<List<CategoryDto>> GetAll()
        {
            var categories = await _categoryRepository.GetAllListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> Get(Guid id)
        {
            var category = await _categoryRepository.AsQueryable()
                .Include(c => c.ProductTypes) // Sử dụng Include để tải ProductTypes
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                throw new ApiException("Category not found.");
            }
            return _mapper.Map<CategoryDto>(category);
        }




    }
}
