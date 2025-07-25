using Microsoft.AspNetCore.Http;
using SharedLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Helpers.HelperServices
{
    public class CurrentUser(IHttpContextAccessor _contextAccessor)
    {
        public string? UserId => _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Sid)?.Value;
        public Guid? UserGuid => UserId != null ? Guid.Parse(UserId) : null;
        public UserRole? UserRole => UserId != null ? (UserRole)byte.Parse(_contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value) : 0;
        public string? UserFullName => _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name).Value;

    }
}
