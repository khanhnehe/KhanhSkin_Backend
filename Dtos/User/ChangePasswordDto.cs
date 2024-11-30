using System.ComponentModel.DataAnnotations;

namespace KhanhSkin_BackEnd.Dtos.User
{
    public class ChangePasswordDto
    {

        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
