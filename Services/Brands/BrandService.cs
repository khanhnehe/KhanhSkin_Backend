using AutoMapper;
using KhanhSkin_BackEnd.Dtos.Brand;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.User;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace KhanhSkin_BackEnd.Services.Brands
{
    public class BrandService : BaseService<Brand, BrandDto, BrandDto, BrandGetRequestInputDto>
    {

        private readonly IConfiguration _config;
        private readonly IRepository<Brand> _brandRepository; // Khởi tạo repository cho User
        private readonly IMapper _mapper; // Khởi tạo AutoMapper để ánh xạ giữa các đối tượng
        private readonly ILogger<BrandService> _logger; // Khởi tạo logger để log thông tin
        private readonly ICloudinary _cloudinary;

        public BrandService(
        IConfiguration config,
        IRepository<Brand> repository,
        IMapper mapper,
        ICloudinary cloudinary,
        ILogger<BrandService> logger,
        ICurrentUser currentUser) // Thêm tham số ICurrentUser
        : base(mapper, repository, logger, currentUser) // Truyền currentUser vào constructor của lớp cơ sở
        {
            _config = config;
            _brandRepository = repository;
            _mapper = mapper;
            _cloudinary = cloudinary;
            _logger = logger;
        }



        public async Task<bool> CheckBrandExist(string brandName)
        {
            return await _brandRepository.AsQueryable().AnyAsync(u => u.BrandName == brandName);
        }

        public override async Task<Brand> Create(BrandDto input)
        {
            if (await CheckBrandExist(input.BrandName))
            {
                throw new ApiException("Brand already exists.");
            }

            var brand = _mapper.Map<Brand>(input);

            if (input.ImageFile != null)
            {
                brand.Image = await UploadImage(input.ImageFile);
            }
            await _brandRepository.CreateAsync(brand);
            await _brandRepository.SaveChangesAsync();

            return brand; // Trả về đối tượng Brand sau khi đã tạo
        }


        public override async Task<Brand> Update(Guid id, BrandDto input)
        {
            var brand = await _brandRepository.AsQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
            if (brand == null)
            {
                throw new ApiException("Không tìm thấy người dùng.");
            }

            if (!string.Equals(brand.BrandName, input.BrandName, StringComparison.OrdinalIgnoreCase) && await CheckBrandExist(input.BrandName))
            {
                throw new ApiException("BrandName đã được sử dụng.");
            }

            _mapper.Map(input, brand);

            if (!string.IsNullOrEmpty(brand.Image) && (input.ImageFile != null || input.ImageFile == null))
            {
                await DeleteImageAsync(brand.Image); // Xóa ảnh cũ
                brand.Image = null; // Đặt lại thuộc tính Image của user
            }


            if (input.ImageFile != null)
            {
                brand.Image = await UploadImage(input.ImageFile); // Tải ảnh mới lên
            }

            await _brandRepository.UpdateAsync(brand);
            await _brandRepository.SaveChangesAsync();


        return brand; // Trả về đối tượng Brand sau khi đã cập nhật
        }


        public async Task DeleteImageAsync(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var publicId = Path.GetFileNameWithoutExtension(imageUrl);
                var deletionParams = new DeletionParams(publicId);
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                if (deletionResult.Error != null)
                {
                    _logger.LogError("Lỗi khi xóa ảnh từ Cloudinary: {0}", deletionResult.Error.Message);
                    throw new ApiException($"Lỗi khi xóa ảnh: {deletionResult.Error.Message}");
                }
            }
        }

        private async Task<string> UploadImage(IFormFile imageFile)
        {
            // Kiểm tra dung lượng và định dạng file
            if (imageFile.Length == 0 || !imageFile.ContentType.StartsWith("image/"))
            {
                throw new ApiException("File ảnh không hợp lệ.");
            }

            // Xử lý tên file để tránh lỗi ký tự đặc biệt
            var sanitizedFileName = Path.GetFileNameWithoutExtension(imageFile.FileName)
                                   .Replace(" ", "_") // Thay thế khoảng trắng bằng dấu gạch dưới
                                   .Replace("?", ""); // Loại bỏ ký tự không hợp lệ

            using (var stream = imageFile.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(sanitizedFileName, stream)
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult == null || uploadResult.Error != null)
                {
                    _logger.LogError("Lỗi Cloudinary: {0}", uploadResult.Error?.Message);
                    throw new ApiException($"Lỗi khi tải ảnh lên: {uploadResult.Error?.Message}");
                }

                // Trả về URL an toàn của ảnh
                return uploadResult.SecureUrl.AbsoluteUri;
            }
        }


    public override async Task<Brand> Delete(Guid id)
        {
            var brand = await _brandRepository.AsQueryable().FirstOrDefaultAsync(a => a.Id == id);
            if (brand == null)
            {
                throw new ApiException("Không tìm thấy thương hiệu.");
            }

        // Kiểm tra và xóa ảnh nếu người dùng có ảnh
            if (!string.IsNullOrEmpty(brand.Image))
            {
                await DeleteImageAsync(brand.Image); // Gọi phương thức DeleteImageAsync để xóa ảnh
            }

        await _brandRepository.DeleteAsync(id);
        await _brandRepository.SaveChangesAsync();


        return brand; // Trả về đối tượng Brand sau khi đã xóa
        }


        public override async Task<List<BrandDto>> GetAll()
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