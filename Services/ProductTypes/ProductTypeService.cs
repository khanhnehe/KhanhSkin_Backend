using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Dtos.ProductType;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.Brands;
using KhanhSkin_BackEnd.Services.CurrentUser;
using Microsoft.EntityFrameworkCore;

namespace KhanhSkin_BackEnd.Services.ProductTypes
{
    public class ProductTypeService : BaseService<ProductType, ProductTypeDto, ProductTypeDto, ProductTypeGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly IRepository<ProductType> _productTypeRepository; 
        private readonly IMapper _mapper; 
        private readonly ILogger<ProductTypeService> _logger;

        public ProductTypeService(
       IConfiguration config,
       IRepository<ProductType> repository,
       IMapper mapper,
       ILogger<ProductTypeService> logger,
       ICurrentUser currentUser) // Thêm tham số ICurrentUser
       : base(mapper, repository, logger, currentUser) // Truyền currentUser vào constructor của lớp cơ sở
        {
            _config = config;
            _productTypeRepository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> CheckTypeExist(string typeName)
        {
            return await _productTypeRepository.AsQueryable().AnyAsync(u => u.TypeName == typeName);
        }


        public async Task<ProductTypeDto> Create(ProductTypeDto input)
        {
            if (await CheckTypeExist(input.TypeName))
            {
                throw new ApiException("TypeName already exists.");
            }

            var productType = _mapper.Map<ProductType>(input);
            await _productTypeRepository.CreateAsync(productType);

            await _productTypeRepository.SaveChangesAsync();

            var newProductType = _mapper.Map<ProductTypeDto>(productType);

            return newProductType;
        }

        public async Task<ProductTypeDto> Update(Guid id, ProductTypeDto input)
        {
            var productType = await _productTypeRepository.AsQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
            if (productType == null)
            {
                throw new ApiException("Không tìm thấy người dùng.");
            }

            // Kiểm tra xem email mới có khác với email hiện tại không và nếu có, kiểm tra xem nó đã được sử dụng bởi người dùng khác chưa
            if (!string.Equals(productType.TypeName, input.TypeName, StringComparison.OrdinalIgnoreCase) && await CheckTypeExist(input.TypeName))
            {
                throw new ApiException("BrandName đã được sử dụng.");
            }

            _mapper.Map(input, productType);
            await _productTypeRepository.UpdateAsync(productType);
            return _mapper.Map<ProductTypeDto>(productType);
        }


        public async Task<List<ProductTypeDto>> GetAll()
        {
            var productTypes = await _productTypeRepository.GetAllListAsync();
            return _mapper.Map<List<ProductTypeDto>>(productTypes);
        }


        public async Task<ProductTypeDto> GetById(Guid id)
        {
            var productType = await _productTypeRepository.AsQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
            if (productType == null)
            {
                throw new ApiException("Không tìm thấy người dùng.");
            }

            return _mapper.Map<ProductTypeDto>(productType);
        }

        public async Task<List<ProductTypeDto>> Search(string typeName)
        {
            var query = _productTypeRepository.AsQueryable();
            if (!string.IsNullOrEmpty(typeName))
            {
                query = query.Where(u => u.TypeName.Contains(typeName));
            }

            var productType = await query.ToListAsync();
            return _mapper.Map<List<ProductTypeDto>>(productType);
        }

        public async Task<bool> Delete(Guid id)
        {
            var productType= await _productTypeRepository.AsQueryable().FirstOrDefaultAsync(a => a.Id == id);
            if (productType == null)
            {
                throw new ApiException("Không tìm thấy thương hiệu."); // Hoặc sử dụng một ngoại lệ tùy chỉnh phù hợp với logic xử lý lỗi của bạn
            }

            await _productTypeRepository.DeleteAsync(id);
            return true; // Nếu không có lỗi, trả về true để báo hiệu việc xóa thành công
        }


    }
}
