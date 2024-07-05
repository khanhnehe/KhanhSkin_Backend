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

namespace KhanhSkin_BackEnd.Services.Users
{
    public class UserService : BaseService<User, UserDto, UserCreateDto, BaseGetRequestInput>
    {
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IRepository<User> _userRepository; // Khởi tạo repository cho User
        private readonly IMapper _mapper; // Khởi tạo AutoMapper để ánh xạ giữa các đối tượng
        private readonly ILogger<UserService> _logger; // Khởi tạo logger để log thông tin
        private readonly ICurrentUser _currentUser; // Khởi tạo dịch vụ CurrentUser để lấy thông tin người dùng hiện tại

        // Constructor nhận các tham số cần thiết cho UserService
        public UserService(
            IRepository<User> repository,
            IMapper mapper,
            ILogger<UserService> logger,
            ICurrentUser currentUser)
            : base(mapper, repository, logger, currentUser) // Gọi constructor của lớp cơ sở với các tham số phù hợp
        {
            _userRepository = repository;
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;
            _passwordHasher = new PasswordHasher<User>(); // Khởi tạo passwordHasher
        }

        // Phương thức Create để tạo người dùng mới
        public async Task<UserDto> Create(UserCreateDto input)
        {
            // Kiểm tra các thông tin bắt buộc
            if (string.IsNullOrWhiteSpace(input.FullName) ||
                string.IsNullOrWhiteSpace(input.Email) ||
                string.IsNullOrWhiteSpace(input.Password))
            {
                throw new ApiException("Missing required request parameters", 400);
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

            // Trả về đối tượng UserDto đã tạo, loại bỏ trường Password
            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        // Implement other interface methods...
    }
}
