using KhanhSkin_BackEnd.Consts;
using KhanhSkin_BackEnd.Entities;
using KhanhSkin_BackEnd.Share.Dtos;

namespace KhanhSkin_BackEnd.Dtos.User
{
    public class UserDto : BaseDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Image { get; set; }
        public Enums.Role Role { get; set; }
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<KhanhSkin_BackEnd.Entities.Address> Addresses { get; set; } = new List<KhanhSkin_BackEnd.Entities.Address>();
        public bool IsActive { get; set; }

    }
}
