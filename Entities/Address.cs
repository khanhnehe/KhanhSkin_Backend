using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhanhSkin_BackEnd.Entities
{
    public class Address : BaseEntity
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tên!")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tỉnh!")]
        public int ProvinceId { get; set; } // ID của tỉnh

        [Required(ErrorMessage = "Vui lòng chọn tỉnh!")]
       public string Province { get; set; } // Tên tỉnh

        [Required(ErrorMessage = "Vui lòng chọn huyện!")]
        public int DistrictId { get; set; } // ID của huyện

        [Required(ErrorMessage = "Vui lòng chọn huyện!")]
        public string District { get; set; } // Tên huyện

        [Required(ErrorMessage = "Vui lòng chọn xã!")]
        public int WardId { get; set; } // ID của xã

        [Required(ErrorMessage = "Vui lòng chọn xã!")]
        public string Ward { get; set; } // Tên xã

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết!")]
        public string AddressDetail { get; set; }

        [Required]
        public Guid UserId { get; set; } // Khóa ngoại liên kết với bảng User

        [ForeignKey("UserId")]
        public User User { get; set; } // Quan hệ với bảng User

        public bool IsDefault { get; set; } = false; // Đánh dấu nếu đây là địa chỉ mặc định
    }
}
