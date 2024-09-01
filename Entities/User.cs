using KhanhSkin_BackEnd.Consts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Entities
{
    public class User : BaseEntity
    {
        [Required(ErrorMessage = "Vui lòng nhập đầy đủ họ tên!")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        public string? PhoneNumber { get; set; }
        public string? Image { get; set; }

        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>(); // Thêm thuộc tính Favorites
        public ICollection<Review> Reviews { get; set; } = new List<Review>(); // Thêm thuộc tính Reviews

        public Enums.Role Role { get; set; } = Enums.Role.User;

        public Cart Cart { get; set; } // Thêm thuộc tính giỏ hàng
        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();

        // Thêm thuộc tính Addresses để thiết lập mối quan hệ 1-n với Address
        public ICollection<Address> Addresses { get; set; } = new List<Address>();

    }
}
