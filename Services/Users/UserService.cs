using AutoMapper;
using KhanhSkin_BackEnd.Dtos.User;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using KhanhSkin_BackEnd.Share.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KhanhSkin_BackEnd.Services.Users
{
    public class UserService : BaseService<User, UserDto, UserCreateDto, BaseGetRequestInput>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly ICurrentUser _currentUser;

        // Thêm các tham số còn thiếu vào constructor
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
        }

        public async Task<User> Create(UserCreateDto input)
        {
            var user = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber,
                Address = input.Address,
                Password = input.Password, // Consider hashing the password before saving
                Image = input.Image,
                Role = input.Role
            };

            await _userRepository.CreateAsync(user);
            await _userRepository.SaveChangesAsync();

            return user;
        }

        // Implement other interface methods...
    }
}
