﻿using AutoMapper;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using KhanhSkin_BackEnd.Consts;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.IO;


namespace KhanhSkin_BackEnd.Services.Users
{
    public class UserService : BaseService<User, UserDto, CreateUpdateUserDto, UserGetRequestInputDto>
    {
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IRepository<User> _userRepository; // Khởi tạo repository cho User
        private readonly IMapper _mapper; // Khởi tạo AutoMapper để ánh xạ giữa các đối tượng
        private readonly ILogger<UserService> _logger; // Khởi tạo logger để log thông tin
        private readonly ICurrentUser _currentUser;
        private readonly ICloudinary _cloudinary;

        // Constructor nhận các tham số cần thiết cho UserService
        public UserService(
            IConfiguration config,
            IRepository<User> repository,
            IMapper mapper,
            ILogger<UserService> logger,    
            ICloudinary cloudinary,
            ICurrentUser currentUser)
            : base(mapper, repository, logger, currentUser) // Gọi constructor của lớp cơ sở với các tham số phù hợp
        {
            _config = config;
            _userRepository = repository;
            _mapper = mapper;
            _logger = logger;
            _cloudinary = cloudinary;
            _currentUser = currentUser;
            _passwordHasher = new PasswordHasher<User>(); // Khởi tạo passwordHasher
        }

        // kiểm tra email đã tồn tại
        public  async Task<bool> CheckEmailExists(string email)
        {
            return await _userRepository.AsQueryable().AnyAsync(u => u.Email == email);
        }

        public async Task<User> CreateUser(CreateUpdateUserDto input)
        {

            // Kiểm tra email đã tồn tại
            if (await CheckEmailExists(input.Email))
            {
                throw new ApiException("Email đã được sử dụng.");
            }

            // Hash mật khẩu trước khi lưu
            var hashedPassword = _passwordHasher.HashPassword(new User(), input.Password);



            // Tạo đối tượng User từ UserCreateDto
            var user = _mapper.Map<User>(input);
            user.Password = hashedPassword;

            if (input.ImageFile != null)
            {
                user.Image = await UploadImage(input.ImageFile);
            }

            // Thêm người dùng vào cơ sở dữ liệu
            await _userRepository.CreateAsync(user);
            // Lưu thay đổi vào cơ sở dữ liệu
            await _userRepository.SaveChangesAsync();

            // Trả về đối tượng User đã tạo
            return user;
        }


        public override async Task<User> Create(CreateUpdateUserDto input)
        {

            // Kiểm tra email đã tồn tại
            if (await CheckEmailExists(input.Email))
            {
                throw new ApiException("Email đã được sử dụng.");
            }

            // Hash mật khẩu trước khi lưu
            var hashedPassword = _passwordHasher.HashPassword(new User(), input.Password);



            // Tạo đối tượng User từ UserCreateDto
            var user = _mapper.Map<User>(input);
            user.Password = hashedPassword;

            if (input.ImageFile != null)
            {
                user.Image = await UploadImage(input.ImageFile);
            }

            // Thêm người dùng vào cơ sở dữ liệu
            await _userRepository.CreateAsync(user);
            // Lưu thay đổi vào cơ sở dữ liệu
            await _userRepository.SaveChangesAsync();

            // Trả về đối tượng User đã tạo
            return user;
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

        //
        private async Task<string> UploadImage(IFormFile imageFile, string? currentImage = null)
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
                try
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

                    // Xóa ảnh cũ (nếu có) sau khi tải ảnh mới thành công
                    if (!string.IsNullOrEmpty(currentImage))
                    {
                        await DeleteImageAsync(currentImage);
                    }

                    // Trả về URL an toàn của ảnh
                    return uploadResult.SecureUrl.AbsoluteUri;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Lỗi khi tải ảnh lên: {0}", ex.Message);
                    throw new ApiException("Có lỗi xảy ra khi tải ảnh lên.");
                }
            }
        }



        //
        public async Task<UserDto> UpdateUser(UpdateUserByIdDto input)
        {
            try
            {
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated.");
                }

                var userEntity = await _userRepository.AsQueryable()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == userId.Value);

                if (userEntity == null)
                {
                    throw new ApiException("Không tìm thấy người dùng.");
                }

                if (!string.Equals(userEntity.Email, input.Email, StringComparison.OrdinalIgnoreCase) &&
                    await CheckEmailExists(input.Email))
                {
                    throw new ApiException("Email đã được sử dụng.");
                }

                userEntity.FullName = input.FullName;
                userEntity.Email = input.Email;

                if (input.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(userEntity.Image))
                    {
                        await DeleteImageAsync(userEntity.Image);
                    }

                    var imageUrl = await UploadImage(input.ImageFile);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        userEntity.Image = imageUrl;
                    }
                }

                await _userRepository.UpdateAsync(userEntity);
                await _userRepository.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(userEntity);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin người dùng.");
                throw new ApiException("Có lỗi xảy ra khi cập nhật thông tin người dùng.");
            }
        }





        public async Task<bool> ChangePassword(string currentPassword, string newPassword)
        {
            var userId = _currentUser.Id;

            // Kiểm tra xem user đã được xác thực chưa
            if (userId == null)
            {
                throw new ApiException("User not authenticated.");
            }

            // Lấy thông tin user hiện tại
            var user = await _userRepository.AsQueryable().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ApiException("User not found.");
            }

            // Kiểm tra mật khẩu hiện tại
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, currentPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                throw new ApiException("Mật khẩu hiện tại không chính xác.");
            }

            // Hash mật khẩu mới
            var hashedNewPassword = _passwordHasher.HashPassword(user, newPassword);

            // Cập nhật mật khẩu mới
            user.Password = hashedNewPassword;
            await _userRepository.UpdateAsync(user);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _userRepository.SaveChangesAsync();

            return true;
        }



        public async Task<UserDto> GetUserById()
        {
            try
            {
                // Lấy ID của người dùng hiện tại từ đối tượng _currentUser
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated");
                }

                // Tìm kiếm người dùng trong cơ sở dữ liệu dựa vào ID
                var user = await _userRepository.GetAsync(userId.Value);
                if (user == null)
                {
                    throw new ApiException("Không tìm thấy người dùng.");
                }

                // Ánh xạ đối tượng User sang UserDto và trả về
                var userDto = _mapper.Map<UserDto>(user);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the user.");
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }



        public async Task<List<UserDto>> GetAllUsers()
        {
            // Lấy danh sách các user có IsActive = true
            var users = await _userRepository.AsQueryable()
                                             .Where(u => u.IsActive)
                                             .ToListAsync();

            // Chuyển đổi sang danh sách UserDto
            return _mapper.Map<List<UserDto>>(users);
        }

        //
        public override async Task<User> Update(Guid id, CreateUpdateUserDto input)
        {
            var user = await _userRepository.AsQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
            if (user == null)
            {
                throw new ApiException("Không tìm thấy người dùng.");
            }

            // Kiểm tra xem email mới có khác với email hiện tại không và nếu có, kiểm tra xem nó đã được sử dụng bởi người dùng khác chưa
            if (!string.Equals(user.Email, input.Email, StringComparison.OrdinalIgnoreCase) && await CheckEmailExists(input.Email))
            {
                throw new ApiException("Email đã được sử dụng.");
            }

            // Cập nhật thông tin user từ input
            _mapper.Map(input, user);

            // Kiểm tra và xử lý ảnh
            if (input.ImageFile != null) // Chỉ xử lý nếu có ảnh mới
            {
                if (!string.IsNullOrEmpty(user.Image))
                {
                    await DeleteImageAsync(user.Image); // Xóa ảnh cũ
                }

                user.Image = await UploadImage(input.ImageFile); // Tải ảnh mới lên
            }

            // Cập nhật thông tin người dùng vào cơ sở dữ liệu
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return user; // Trả về đối tượng User sau khi đã cập nhật
        }


        public override async Task<User> Delete(Guid id)
        {
            try
            {
                // Tìm người dùng bằng ID, bỏ qua các bộ lọc truy vấn (nếu có)
                var user = await _userRepository.AsQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
                if (user == null)
                {
                    throw new ApiException("Không tìm thấy người dùng.");
                }

                // Chuyển trạng thái IsActive sang false thay vì xóa
                user.IsActive = false;

                // Cập nhật thay đổi vào cơ sở dữ liệu
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync(); // Lưu các thay đổi vào cơ sở dữ liệu

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", id);
                throw;
            }
        }





        public async Task<string> SignIn(SignInDto input)
        {
            // Tìm user theo email
            var user = await _userRepository.AsQueryable().FirstOrDefaultAsync(u => u.Email == input.Email);

            if (user == null)
            {
                throw new ApiException("Email không tồn tại.");
            }

            // Kiểm tra nếu user không hoạt động
            if (!user.IsActive)
            {
                throw new ApiException("Tài khoản của bạn đã bị vô hiệu hóa.");
            }

            // Kiểm tra mật khẩu
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, input.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new ApiException("Mật khẩu không đúng.");
            }

            // Tạo JWT và trả về
            return GenerateJWT(user);
        }





        private string GenerateJWT(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Thêm NameIdentifier claim
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString()), // Chuyển đổi Role sang string
        new Claim("FullName", user.FullName ?? string.Empty),
        new Claim("Image", user.Image ?? string.Empty),
    };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.AddHours(7).AddDays(30),
                signingCredentials: credentials,
                claims: claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }



        //public override IQueryable<User> CreateFilteredQuery(UserGetRequestInputDto input)
        //{
        //    var query = base.CreateFilteredQuery(input);

        //    if (!string.IsNullOrEmpty(input.FreeTextSearch))
        //    {
        //        input.FreeTextSearch = input.FreeTextSearch.Trim().ToLower();
        //        query = query.Where(a => a.FullName.ToLower().Contains(input.FreeTextSearch)
        //                                 || a.Email.ToLower().Contains(input.FreeTextSearch)
        //                                 || a.PhoneNumber.ToLower().Contains(input.FreeTextSearch));
        //    }

        //    if (input.Role.HasValue)
        //    {
        //        query = query.Where(a => a.Role == input.Role.Value);
        //    }

        //    return query;
        //}
    }
}
