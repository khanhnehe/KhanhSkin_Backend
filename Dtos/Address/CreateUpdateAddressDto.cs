using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.Address
{
    public class CreateUpdateAddressDto : BaseDto
         {
        public string PhoneNumber { get; set; }

        public int ProvinceId { get; set; } // ID của tỉnh
        public string Province { get; set; } // Tên tỉnh

        public int DistrictId { get; set; } // ID của huyện

        public string District { get; set; } // Tên huyện

        public int WardId { get; set; } // ID của xã

        public string Ward { get; set; } // Tên xã

        public string AddressDetail { get; set; }


        public bool IsDefault { get; set; } = false; // Đánh dấu nếu đây là địa chỉ mặc định
    }

}


