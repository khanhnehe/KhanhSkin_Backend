using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Share.Dtos;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.User
{
    public class UserCreateDto : BaseDto
    {
        public UserCreateDto()
        {
            Role = Enums.Role.User;
        }

        [Required(ErrorMessage = "Vui lòng nhập đầy đủ họ tên!")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò!")]

        public string? Image { get; set; }

        public Enums.Role Role { get; set; }
    }
}
