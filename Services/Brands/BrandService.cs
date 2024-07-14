using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Services.Users;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.AspNetCore.Identity;

namespace KhanhSkin_BackEnd.Services.Brands
{
    public class BrandService : BaseService<Brand, BrandDto, BrandDto, BaseGetRequestInput>
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

    }
}
