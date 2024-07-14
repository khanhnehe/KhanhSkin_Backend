using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using KhanhSkin_BackEnd.Consts;

namespace KhanhSkin_BackEnd.Services.CurrentUser
{
    // Lớp CurrentUser thực thi interface ICurrentUser, cung cấp thông tin về người dùng hiện tại.
    public class CurrentUser : ICurrentUser
    {
        // Sử dụng IHttpContextAccessor để truy cập HttpContext, từ đó lấy được thông tin người dùng.
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor nhận IHttpContextAccessor làm tham số và khởi tạo _httpContextAccessor.
        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Thuộc tính Id trả về ID của người dùng hiện tại dưới dạng Guid.
        // Nếu không tìm thấy hoặc không thể chuyển đổi thành Guid, trả về null.
        public Guid? Id
        {
            get
            {
                var idString = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(idString) && Guid.TryParse(idString, out var id))
                {
                    return id;
                }
                return null;
            }
        }

        // Thuộc tính Role trả về vai trò của người dùng hiện tại dưới dạng Enums.Role.
        // Nếu không tìm thấy hoặc không thể chuyển đổi thành Enums.Role, trả về null.
        public Enums.Role? Role
        {
            get
            {
                var roleString = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
                if (!string.IsNullOrEmpty(roleString) && Enum.TryParse<Enums.Role>(roleString, out var role))
                {
                    return role;
                }
                return null;
            }
        }

        // Thuộc tính FullName trả về tên đầy đủ của người dùng hiện tại.
        // Nếu không tìm thấy, trả về null.
        public string? FullName => _httpContextAccessor?.HttpContext?.User.FindFirstValue("FullName");
    }
}
