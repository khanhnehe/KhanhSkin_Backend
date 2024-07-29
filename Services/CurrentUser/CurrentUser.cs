using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using KhanhSkin_BackEnd.Consts;

namespace KhanhSkin_BackEnd.Services.CurrentUser
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentUser> _logger;

        public CurrentUser(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUser> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Guid? Id
        {
            get
            {
                var idString = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(idString) && Guid.TryParse(idString, out var id))
                {
                    _logger.LogInformation($"User ID found: {id}");
                    return id;
                }
                _logger.LogWarning("User ID not found or invalid");
                return null;
            }
        }

        public Enums.Role? Role
        {
            get
            {
                var roleString = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
                if (!string.IsNullOrEmpty(roleString) && Enum.TryParse(roleString, out Enums.Role role))
                {
                    _logger.LogInformation($"User role found: {role}");
                    return role;
                }
                _logger.LogWarning("User role not found or invalid");
                return null;
            }
        }

        public string? FullName
        {
            get
            {
                var fullName = _httpContextAccessor?.HttpContext?.User?.FindFirstValue("FullName");
                if (!string.IsNullOrEmpty(fullName))
                {
                    _logger.LogInformation($"User full name found: {fullName}");
                    return fullName;
                }
                _logger.LogWarning("User full name not found");
                return null;
            }
        }
    }
}
