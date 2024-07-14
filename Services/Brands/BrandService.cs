﻿using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Services.Users;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.User;

namespace KhanhSkin_BackEnd.Services.Brands
{
    public class BrandService : BaseService<Brand, BrandDto, BrandDto, BrandGetRequestInputDto>
    {

        private readonly IConfiguration _config;
        private readonly IRepository<Brand> _brandRepository; // Khởi tạo repository cho User
        private readonly IMapper _mapper; // Khởi tạo AutoMapper để ánh xạ giữa các đối tượng
        private readonly ILogger<BrandService> _logger; // Khởi tạo logger để log thông tin

        public BrandService(
        IConfiguration config,
        IRepository<Brand> repository,
        IMapper mapper,
        ILogger<BrandService> logger,
        ICurrentUser currentUser) // Thêm tham số ICurrentUser
        : base(mapper, repository, logger, currentUser) // Truyền currentUser vào constructor của lớp cơ sở
        {
            _config = config;
            _brandRepository = repository;
            _mapper = mapper;
            _logger = logger;
        }



        public async Task<bool> CheckBrandExist(string brandName)
        {
            return await _brandRepository.AsQueryable().AnyAsync(u => u.BrandName == brandName);
        }

        public async Task<BrandDto> Create(BrandDto input)
        {
            if (await CheckBrandExist(input.BrandName))
            {
                throw new ApiException("Brand already exists.");
            }

            // Sử dụng AutoMapper để chuyển đổi từ DTO (Data Transfer Object) sang entity Brand
            var brand = _mapper.Map<Brand>(input);
            await _brandRepository.CreateAsync(brand);

            await _brandRepository.SaveChangesAsync();

            // Sau khi thương hiệu được tạo, chuyển đổi entity Brand trở lại thành DTO để trả về
            var newBrand = _mapper.Map<BrandDto>(brand);

            return newBrand;
        }

        public async Task<BrandDto> Update(Guid id, BrandDto input)
        {
            var brand = await _brandRepository.AsQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
            if (brand == null)
            {
                throw new ApiException("Không tìm thấy người dùng.");
            }

            // Kiểm tra xem email mới có khác với email hiện tại không và nếu có, kiểm tra xem nó đã được sử dụng bởi người dùng khác chưa
            if (!string.Equals(brand.BrandName, input.BrandName, StringComparison.OrdinalIgnoreCase) && await CheckBrandExist(input.BrandName))
            {
                throw new ApiException("BrandName đã được sử dụng.");
            }

            _mapper.Map(input, brand);
            await _brandRepository.UpdateAsync(brand);
            return _mapper.Map<BrandDto>(brand);
        }

        public async Task<bool> Delete(Guid id)
        {
            var brand = await _brandRepository.DeleteAsync(id);
            return brand != null;
        }

        public async Task<List<BrandDto>> GetAll()
        {
            var brands = await _brandRepository.GetAllListAsync();
            return _mapper.Map<List<BrandDto>>(brands);
        }

        public async Task<BrandDto> GetById(Guid id)
        {
            var brand = await _brandRepository.AsQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
            if (brand == null)
            {
                throw new ApiException("Không tìm thấy người dùng.");
            }
            return _mapper.Map<BrandDto>(brand);
        }

        public async Task<List<BrandDto>> Search(string keyword)
        {
            var brands = await _brandRepository
                .AsQueryable()
                .Where(b => b.BrandName.Contains(keyword))
                .ToListAsync();

            return _mapper.Map<List<BrandDto>>(brands);
        }

    }
}