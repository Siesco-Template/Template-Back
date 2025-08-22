using Auth.Business.Dtos;
using Auth.Business.Services;
using Auth.Business.Utilies.PasswordUtilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Attributes;
using SharedLibrary.Enums;
using SharedLibrary.StaticDatas;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AuthService _authService) : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            await _authService.RegisterAsync(dto);
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            return Ok(await _authService.LoginAsync(dto));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> LoginWithRefreshToken(string refreshToken)
        {
            return Ok(await _authService.LoginWithRefreshTokenAsync(refreshToken));
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            await _authService.ChangePasswordAsync(dto);
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            await _authService.ForgetPasswordAsync(email);
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SetPassword(SetPasswordDto dto)
        {
            await _authService.SetPasswordAsync(dto);
            return Ok();
        }

        [Authorize]
        [Permission(PageKeys.User, ActionKeys.Create)]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            await _authService.CreateUserAsync(dto);
            return Ok();
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateUser(UpdateUserDto dto)
        {
            await _authService.UpdateUserAsync(dto);
            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetUserDetailAsync(Guid userId)
        {
            return Ok(await _authService.GetUserDetailAsync(userId));
        }

        //Burada verilməlidir admin ve superadmin(UserRoleAttribute)
        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> ToggleBlockUser(ToggleBlockUserDto dto)
        {
            await _authService.ToggleBlockUserAsync(dto);
            return Ok();
        }

        [HttpGet("[action]")]
        public string GeneratePassword()
        {
            return PasswordChecker.GenerateRandomPassword();
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> ResetPasswordWithEmail(Guid userId)
        {
            await _authService.ResetPasswordWithEmailAsync(userId);
            return Ok();
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CheckUserExist(string email ,string phoneNumber)
        {
            await _authService.CheckUserExistAsync(email ,phoneNumber);
            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUsersForPermission()
        {
            return Ok(await _authService.GetAllUsersForPermissionAsync());
        }

    }
}
