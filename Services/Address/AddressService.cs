using AutoMapper;
using AutoWrapper.Wrappers;
using KhanhSkin_BackEnd.Dtos.Address;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Repositories;
using KhanhSkin_BackEnd.Services.CurrentUser;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KhanhSkin_BackEnd.Dtos.User;


using Addresses = KhanhSkin_BackEnd.Entities.Address;

namespace KhanhSkin_BackEnd.Services.Address
{
    public class AddressService : BaseService<Addresses, AddressDto, CreateUpdateAddressDto, UserGetRequestInputDto>
    {
        private readonly IRepository<Addresses> _addressRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AddressService> _logger;
        private readonly ICurrentUser _currentUser;
        private readonly LocationService _locationService;

        public AddressService(
            IConfiguration config,
            IRepository<Addresses> repository,
            IMapper mapper,
            ILogger<AddressService> logger,
            ICurrentUser currentUser,
            LocationService locationService)
            : base(mapper, repository, logger, currentUser)
        {
            _addressRepository = repository;
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;
            _locationService = locationService;
        }

        public override async Task<Addresses> Create(CreateUpdateAddressDto input)
        {
            try
            {
                // Lấy ID người dùng hiện tại
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated");
                }

                // Nếu địa chỉ mới là mặc định, đặt tất cả các địa chỉ khác của người dùng thành không mặc định
                if (input.IsDefault)
                {
                    var existingAddresses = await _addressRepository.AsQueryable()
                        .Where(a => a.UserId == userId.Value && a.IsDefault)
                        .ToListAsync();

                    foreach (var address in existingAddresses)
                    {
                        address.IsDefault = false;
                        await _addressRepository.UpdateAsync(address);
                    }
                }

                // Lấy thông tin tên tỉnh, huyện, xã từ LocationService dựa vào ID
                input.Province = await _locationService.GetProvinceAsync(input.ProvinceId);
                input.District = await _locationService.GetDistrictAsync(input.DistrictId);
                input.Ward = await _locationService.GetWardAsync(input.WardId);

                // Ánh xạ từ DTO sang thực thể Address
                var addressEntity = _mapper.Map<Addresses>(input);
                addressEntity.UserId = userId.Value;

                // Thêm địa chỉ vào cơ sở dữ liệu
                await _addressRepository.CreateAsync(addressEntity);
                await _addressRepository.SaveChangesAsync();

                return addressEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                throw new ApiException("Có lỗi xảy ra khi tạo địa chỉ.");
            }
        }

        public override async Task<Addresses> Update(Guid id, CreateUpdateAddressDto input)
        {
            try
            {
                var addressEntity = await _addressRepository.GetAsync(id);
                if (addressEntity == null)
                {
                    throw new ApiException("Không tìm thấy địa chỉ.");
                }

                // Lấy ID người dùng hiện tại
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new Exception("User not authenticated");
                }

                // Nếu địa chỉ cập nhật là mặc định, đặt tất cả các địa chỉ khác của người dùng thành không mặc định
                if (input.IsDefault)
                {
                    var existingAddresses = await _addressRepository.AsQueryable()
                        .Where(a => a.UserId == userId.Value && a.IsDefault && a.Id != id)
                        .ToListAsync();

                    foreach (var address in existingAddresses)
                    {
                        address.IsDefault = false;
                        await _addressRepository.UpdateAsync(address);
                    }
                }

                // Cập nhật thông tin địa chỉ từ input
                input.Province = await _locationService.GetProvinceAsync(input.ProvinceId);
                input.District = await _locationService.GetDistrictAsync(input.DistrictId);
                input.Ward = await _locationService.GetWardAsync(input.WardId);

                _mapper.Map(input, addressEntity);

                await _addressRepository.UpdateAsync(addressEntity);
                await _addressRepository.SaveChangesAsync();

                return addressEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address");
                throw new ApiException("Có lỗi xảy ra khi cập nhật địa chỉ.");
            }
        }

        public override async Task<Addresses> Delete(Guid id)
        {
            try
            {
                var address = await _addressRepository.GetAsync(id);
                if (address == null)
                {
                    throw new ApiException("Không tìm thấy địa chỉ.");
                }

                // Kiểm tra xem địa chỉ có phải là địa chỉ mặc định không
                if (address.IsDefault)
                {
                    throw new ApiException("Không thể xóa địa chỉ mặc định.");
                }

                await _addressRepository.DeleteAsync(id);
                await _addressRepository.SaveChangesAsync();

                return address;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address");
                throw new ApiException($"{ex.Message}");
            }
        }


        public async Task<List<AddressDto>> GetAllAddressesAsync()
        {
            var addresses = await _addressRepository.GetAllListAsync();
            return _mapper.Map<List<AddressDto>>(addresses);
        }

        public async Task<List<AddressDto>> GetAllAddressesByUserId()
        {
            try
            {
                // Lấy ID của người dùng hiện tại
                var userId = _currentUser.Id;
                if (userId == null)
                {
                    throw new ApiException("User not authenticated");
                }

                // Tìm kiếm tất cả các địa chỉ cho người dùng hiện tại
                var addresses = await _addressRepository
                    .AsQueryable()
                    .Include(a => a.User)  // Nếu cần tham chiếu đến bảng User
                    .Where(a => a.UserId == userId.Value)
                    .ToListAsync();

                if (addresses == null )
                {
                    throw new ApiException("Không tìm thấy địa chỉ của người dùng.");
                }

                // Ánh xạ danh sách đối tượng Address sang danh sách AddressDto
                var addressDtos = _mapper.Map<List<AddressDto>>(addresses);
                return addressDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the addresses for the current user.");
                throw new ApiException($"An error occurred: {ex.Message}");
            }
        }




    }
}
