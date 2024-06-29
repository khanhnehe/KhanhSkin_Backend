using KhanhSkin_BackEnd.Consts;

namespace KhanhSkin_BackEnd.Services.CurrentUser
{
    public interface ICurrentUser
    {
        Guid? Id { get; }
        Enums.Role? Role { get; }
        string? FullName { get; }
    }
}
