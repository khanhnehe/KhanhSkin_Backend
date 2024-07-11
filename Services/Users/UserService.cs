using AutoMapper;
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

namespace KhanhSkin_BackEnd.Services.Users
{
    public class UserService : BaseService<User, UserDto, CreateUpdateUserDto, BaseGetRequestInput>
    {
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IRepository<User> _userRepository; // Khởi tạo repository cho User
        private readonly IMapper _mapper; // Khởi tạo AutoMapper để ánh xạ giữa các đối tượng
        private readonly ILogger<UserService> _logger; // Khởi tạo logger để log thông tin
        private readonly ICurrentUser _currentUser; // Khởi tạo dịch vụ CurrentUser để lấy thông tin người dùng hiện tại

        // Constructor nhận các tham số cần thiết cho UserService
        public UserService(
            IConfiguration config,
            IRepository<User> repository,
            IMapper mapper,
            ILogger<UserService> logger,
            ICurrentUser currentUser)
            : base(mapper, repository, logger, currentUser) // Gọi constructor của lớp cơ sở với các tham số phù hợp
        {
            _config = config;
            _userRepository = repository;
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;
            _passwordHasher = new PasswordHasher<User>(); // Khởi tạo passwordHasher
        }

        // kiểm tra email đã tồn tại
        public async Task<bool> CheckEmailExists(string email)
        {
            return await _userRepository.AsQueryable().AnyAsync(u => u.Email == email);
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

            // Thêm người dùng vào cơ sở dữ liệu
            await _userRepository.CreateAsync(user);
            // Lưu thay đổi vào cơ sở dữ liệu
            await _userRepository.SaveChangesAsync();

            // Trả về đối tượng User đã tạo
            return user;
        }

        [Authorize]
        public async Task<bool> ChangePassword(string email, string currentPassword, string newPassword)
        {
            var user = await _userRepository.AsQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new ApiException("Không tìm thấy người dùng.");
            }

            // Kiểm tra mật khẩu hiện tại
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, currentPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                throw new ApiException("Mật khẩu hiện tại không chính xác.");
            }

            // Hash mật khẩu mới và cập nhật
            user.Password = _passwordHasher.HashPassword(user, newPassword);
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<UserDto> GetUserById(Guid id)
        {
            var user = await _userRepository.GetAsync(id);
            if (user == null)
            {
                throw new ApiException("Không tìm thấy người dùng.");
            }
            return _mapper.Map<UserDto>(user);
        }


        public async Task<List<UserDto>> GetAllUsers()
        {
            var users = await _userRepository.GetAllListAsync();
            return _mapper.Map<List<UserDto>>(users);
        }


        public async Task<UserDto> UpdateUser(Guid id, CreateUpdateUserDto input)
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
            // Lưu ý: Không cập nhật mật khẩu ở đây. Nếu cần thay đổi mật khẩu, nên có một phương thức riêng biệt.

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }


        public async Task<bool> DeleteUser(Guid id)
        {
            // Tìm người dùng bằng ID, bỏ qua các bộ lọc truy vấn (nếu có)
            var user = await _userRepository.AsQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
            if (user == null)
            {
                throw new ApiException("Không tìm thấy người dùng.");
            }

            // Xóa người dùng và lưu thay đổi
            await _userRepository.DeleteByEntityAsync(user);
            return true;
        }



        public async Task<string> SignIn(SignInDto input)
        {
            var user = await _userRepository.AsQueryable().FirstOrDefaultAsync(u => u.Email == input.Email);
            if (user == null)
            {
                throw new ApiException("Email không tồn tại.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, input.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new ApiException("Mật khẩu không đúng.");
            }

            return GenerateJWT(user);
        }




        private string GenerateJWT(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()), // Chuyển đổi Role sang string
                new Claim("Id", user.Id.ToString()),
                new Claim("FullName", user.FullName ?? string.Empty),
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

    }
}
