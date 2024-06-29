using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using KhanhSkin_BackEnd.Consts;

namespace KhanhSkin_BackEnd.Services.CurrentUser
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

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

        public string? FullName => _httpContextAccessor?.HttpContext?.User.FindFirstValue("FullName");

    }
}
