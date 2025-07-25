using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SharedLibrary.HelperServices
{
    public class CurrentUser(IHttpContextAccessor _contextAccessor)
    {
        public string UserId => _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Sid)?.Value ?? throw new UnauthorizedAccessException("User is not authenticated.");
        public Guid UserGuid => Guid.Parse(_contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //public string UserName => _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        //public byte UserStatus => Convert.ToByte(_contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role));
        public string BaseUrl => $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host.Value}{_contextAccessor.HttpContext.Request.PathBase.Value}";
    }
}   