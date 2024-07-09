using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.User
{
    public class SignInDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
