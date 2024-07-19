using KhanhSkin_BackEnd.Consts;
using System;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Entities
{
    public class User : BaseEntity
    {
        //public Guid Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đầy đủ họ tên!")]
        public string FullName { get; set; }

       

        [Required(ErrorMessage = "Vui lòng nhập email!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Image { get; set; }

        public ICollection<Favorite> Favorites { get; set; }


        public Enums.Role Role { get; set; } = Enums.Role.User;


    }
}
