using KhanhSkin_BackEnd.Consts;
using System;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Entities
{
    public class User : BaseEntity
    {
        //public Guid Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ!")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Image { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò!")]
        public Enums.Role Role { get; set; }
    }
}
