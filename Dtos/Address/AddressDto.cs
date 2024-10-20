using KhanhSkin_BackEnd.Share.Dtos;
using System;

namespace KhanhSkin_BackEnd.Dtos.Address
{
    public class AddressDto : BaseDto
    {
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public int ProvinceId { get; set; } // ID của tỉnh
        public string Province { get; set; }
        public int DistrictId { get; set; } // ID của 
        public string District { get; set; } // Tên huyện
        public int WardId { get; set; } // ID của xã

        public string Ward { get; set; } // Tên xã
        public string AddressDetail { get; set; }
        public Guid UserId { get; set; }
        public bool IsDefault { get; set; } = false; // Đánh dấu nếu đây là địa chỉ mặc định
    }
}
