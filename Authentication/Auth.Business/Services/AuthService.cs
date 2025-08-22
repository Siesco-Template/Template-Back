using Auth.Business.Dtos;
using Auth.Business.Exceptions;
using Auth.Business.Helpers.HelperServices;
using Auth.Business.Helpers.HelperServices.Email;
using Auth.Business.Helpers.HelperServices.Token;
using Auth.Business.Utilies.PasswordUtilities;
using Auth.Core.Entities;
using Auth.DAL.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Dtos.PermissionDtos;
using SharedLibrary.Enums;
using SharedLibrary.Events;
using SharedLibrary.Exceptions;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Business.Services
{
    public class AuthService(AuthDbContext _context, TokenService _tokenService, EmailService _emailService, CurrentUser _currentUser, IPublishEndpoint _publish)
    {
        public async Task RegisterAsync(RegisterDto dto)
        {
            PasswordChecker.CheckPasswordAndThrowException(dto.Password.Trim());

            dto.PhoneNumber = dto.PhoneNumber.Trim();
            dto.Email = dto.Email.Trim();

            await CheckUserExistAsync(dto.Email, dto.PhoneNumber);

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName!.Trim(),
                LastName = dto.LastName!.Trim(),
                Password = _tokenService.GeneratePasswordHash(dto.Password.Trim()),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                UserRole = UserRole.SimpleUser,
                RegistrationDate = DateTime.Now,
                SignatureNumber = dto.SignatureNumber
            };

            await _context.AppUsers.AddAsync(user);
            await _context.SaveChangesAsync();

            ////TODO : bu hisse frontdan template istenilmeli ve ona uygun yazilmalidir
            //await _emailService.SendRegister(user.Email, $"{user.FirstName} {user.LastName}");

            ////Burada publisher olacaq
            //await _publish.Publish(new CreateUserEvent
            //{
            //    Email = user.Email,
            //    FirstName = user.FirstName,
            //    LastName = user.LastName,
            //    PhoneNumber = user.PhoneNumber,
            //    UserRole = user.UserRole,
            //    UserId = user.Id,
            //    SignatureNumber = user.SignatureNumber,
            //    RegistrationDate = user.RegistrationDate
            //});

            await _publish.Publish(new UserRegisteredPermissionEvent
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                IsBlocked = false,
                Pages = new List<PageDto>()
            });
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Email == dto.EmailOrUserName)
                ?? throw new LoginFailedException();

            if (user.Password != _tokenService.GeneratePasswordHash(dto.Password))
                throw new LoginFailedException();

            if (user.IsBlock)
            {
                if (user.LockDownDate != null && user.LockDownDate > DateTime.Now)
                {
                    throw new LoginFailedException(user.BlockInformation + " səbəbinə görə" + user.LockDownDate.ToString() + " tarixinə qədər bloklanmısınız");
                }
                else if (user.LockDownDate == null)
                {
                    throw new LoginFailedException("Siz bloklanmısınız : " + user.BlockInformation);
                }
            }

            var accessToken = _tokenService.CreateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(accessToken, 1440);

            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpireDate = refreshToken.Expires;
            await _context.SaveChangesAsync();

            var responseDto = new TokenResponseDto
            {
                UserId = user.Id.ToString(),
                FullName = user.FirstName + " " + user.LastName,
                UserRole = user.UserRole,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                Expires = refreshToken.Expires,
            };

            return responseDto;
        }

        public async Task<TokenResponseDto> LoginWithRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                throw new NotFoundException("Token boş ola bilməz");

            var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken) ??
                throw new NotFoundException("İstifadəçi mövcud deyil");

            if (user == null)
                throw new LoginFailedException();

            if (user.RefreshTokenExpireDate < DateTime.Now)
                throw new BadRequestException("Yenidən giriş etmək lazımdır");

            var newToken = _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(newToken, 1440);

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpireDate = newRefreshToken.Expires;
            await _context.SaveChangesAsync();

            var responseDto = new TokenResponseDto
            {
                UserId = user.Id.ToString(),
                FullName = user.FirstName + " " + user.LastName,
                AccessToken = newToken,
                RefreshToken = newRefreshToken.Token,
                UserRole = user.UserRole,
                Expires = newRefreshToken.Expires,
            };

            return responseDto;
        }

        public async Task ChangePasswordAsync(ChangePasswordDto dto)
        {
            PasswordChecker.CheckPasswordAndThrowException(dto.NewPassword.Trim());
            PasswordChecker.CheckPasswordAndThrowException(dto.NewConfirmPassword.Trim());

            if(dto.OldPassword == dto.NewPassword)
                throw new BadRequestException("Yeni şifrəniz köhnə ilə eynidir.Fərqli şifrə daxil edin");

            if (dto.NewPassword != dto.NewConfirmPassword)
                throw new BadRequestException("Şifrələr uyğunlaşmır");

            var user = await GetUserById((Guid)_currentUser.UserGuid);

            if (user.Password != _tokenService.GeneratePasswordHash(dto.OldPassword))
                throw new BadRequestException("Köhnə şifrə doğru deyil");

            user.Password = _tokenService.GeneratePasswordHash(dto.NewPassword);
            await _context.SaveChangesAsync();
        }

        public async Task ForgetPasswordAsync(string email)
        {
            var user = await _context.AppUsers.Include(x => x.PasswordToken).FirstOrDefaultAsync(x => x.Email == email)
                                    ?? throw new BadRequestException("İstifadəçi mövcüd deyil!");
            if (user != null)
            {
                if (user.PasswordToken != null)
                {
                    _context.Remove(user.PasswordToken);
                    await _context.SaveChangesAsync();
                }
                var token = _tokenService.CreatePasswordResetToken(user);

                await _context.PasswordTokens.AddAsync(new PasswordToken
                {
                    Id = Guid.NewGuid(),
                    Token = token,
                    AppUserId = user.Id,
                    ExpireDate = DateTime.UtcNow.AddHours(1)
                });

                await _context.SaveChangesAsync();

                await _emailService.SendResetPassword(user.Email, $"{user.FirstName} {user.LastName}", token);
            }
        }

        public async Task SetPasswordAsync(SetPasswordDto dto)
        {
            PasswordChecker.CheckPasswordAndThrowException(dto.NewPassword.Trim());
            PasswordChecker.CheckPasswordAndThrowException(dto.ConfirmNewPassword.Trim());

            var handler = new JwtSecurityTokenHandler();

            var jwtToken = handler.ReadJwtToken(dto.Token);

            string? email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            AppUser user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email)
                ?? throw new NotFoundException("İstifadəçi mövcud deyil");

            var passwordToken = await _context.PasswordTokens.FirstOrDefaultAsync(pt =>
               pt.Token == dto.Token && pt.AppUserId == user.Id && pt.ExpireDate > DateTime.Now) ?? throw new BadRequestException();

            string newPassword = _tokenService.GeneratePasswordHash(dto.NewPassword);

            if (user.Password == newPassword) throw new BadRequestException("Yeni şifrəniz köhnə ilə eynidir.Fərqli şifrə daxil edin");

            user.Password = newPassword;

            _context.PasswordTokens.Remove(passwordToken);
            await _context.SaveChangesAsync();
        }

        public async Task CreateUserAsync(CreateUserDto dto)
        {
            dto.PhoneNumber = dto.PhoneNumber.Trim();
            dto.Email = dto.Email.Trim();

            var existUser = await _context.AppUsers.Where(x => x.Email == dto.Email || x.PhoneNumber == dto.PhoneNumber)
            .Select(x => new
            {
                x.Email,
                x.PhoneNumber
            }).FirstOrDefaultAsync();

            if (existUser != null)
            {
                if (existUser.Email == dto.Email)
                    throw new BadRequestException("Email mövcuddur!");
                if (existUser.PhoneNumber == dto.PhoneNumber)
                    throw new BadRequestException("Telefon nömrəsi mövcuddur!");
            }

            string password = PasswordChecker.GenerateRandomPassword();

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName!.Trim(),
                LastName = dto.LastName!.Trim(),
                Password = _tokenService.GeneratePasswordHash(password),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                UserRole = dto.UserRole,
                RegistrationDate = DateTime.Now,
                SignatureNumber = dto.SignatureNumber
            };

            await _context.AppUsers.AddAsync(user);
            await _context.SaveChangesAsync();

            //await _emailService.SendCreatedByAdmin(user.Email, $"{user.FirstName} {user.LastName}", password);

            //Burada publisher olacaq
            //await _publish.Publish(new CreateUserEvent
            //{
            //    Email = user.Email,
            //    FirstName = user.FirstName,
            //    LastName = user.LastName,
            //    PhoneNumber = user.PhoneNumber,
            //    UserRole = user.UserRole,
            //    UserId = user.Id,
            //    SignatureNumber = user.SignatureNumber,
            //    RegistrationDate = user.RegistrationDate
            //});

            //var token = _tokenService.CreatePasswordResetToken(user);

            //await _context.PasswordTokens.AddAsync(new PasswordToken
            //{
            //    Id = Guid.NewGuid(),
            //    Token = token,
            //    AppUserId = user.Id,
            //    ExpireDate = DateTime.Now.AddHours(8)
            //});
            //await _context.SaveChangesAsync();

            await _publish.Publish(new UserRegisteredPermissionEvent
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                IsBlocked = false,
                Pages = new List<PageDto>()
            });
        }

        public async Task ToggleBlockUserAsync(ToggleBlockUserDto dto)
        {
            //Bu hisseye atribute verildiyi zaman ehtiyyac qalmır
            //if (_currentUser.UserRole != AppUserRole.Admin || _currentUser.UserRole != AppUserRole.SuperAdmin)
            //    throw new PermissionDeniedException();

            var user = await GetUserById(dto.UserId);

            if (!user.IsBlock)
            {
                user.IsBlock = true;
                user.BlockInformation = dto.BlockInformation != null ? dto.BlockInformation.Trim() : null;
                user.LockDownDate = dto.LockDownDate;
            }
            else
            {
                user.IsBlock = false;
                user.BlockInformation = null;
                user.LockDownDate = null;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<UserDetailDto> GetUserDetailAsync(Guid userId)
        {
            var user = await _context.AppUsers
                .Where(x => x.Id == userId)
                .Select(x => new UserDetailDto
                {
                    Id = x.Id,
                    BlockInformation = x.BlockInformation,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    IsBlock = x.IsBlock,
                    LastName = x.LastName,
                    LockDownDate = x.LockDownDate,
                    PhoneNumber = x.PhoneNumber,
                    UserRole = x.UserRole
                })
                .FirstOrDefaultAsync() ?? throw new NotFoundException("İstifadəçi mövcud deyil");

            return user;
        }

        public async Task<AppUser> GetUserById(Guid UserId)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Id == UserId) ??
                throw new NotFoundException("İstifadəçi mövcud deyil");

            return user;
        }

        public async Task UpdateUserAsync(UpdateUserDto dto)
        {
            var user = await GetUserById(dto.UserId);

            dto.Email = dto.Email.Trim();
            dto.PhoneNumber = dto.PhoneNumber.Trim();

            var existUser = await _context.AppUsers.Where(x => x.Id != dto.UserId && (x.Email == dto.Email || x.PhoneNumber == dto.PhoneNumber))
            .Select(x => new
            {
                x.Email,
                x.PhoneNumber
            }).FirstOrDefaultAsync();

            if (existUser != null)
            {
                if (existUser.Email == dto.Email)
                    throw new BadRequestException("Email mövcuddur!");
                if (existUser.PhoneNumber == dto.PhoneNumber)
                    throw new BadRequestException("Telefon nömrəsi mövcuddur!");
            }

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.PhoneNumber = dto.PhoneNumber.Trim();
            user.Email = dto.Email.Trim();
            user.UserRole = dto.UserRole;

            await _context.SaveChangesAsync();
        }

        public async Task ResetPasswordWithEmailAsync(Guid UserId)
        {
            var user = await GetUserById(UserId);

            var password = PasswordChecker.GenerateRandomPassword();

            user.Password = _tokenService.GeneratePasswordHash(password);

            await _context.SaveChangesAsync();

            await _emailService.SendChangedPasswordByAdmin(user.Email, $"{user.FirstName} {user.LastName}", password);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            dto.NewPassword = dto.NewPassword.Trim();
            PasswordChecker.CheckPasswordAndThrowException(dto.NewPassword);

            var user = await GetUserById(dto.UserId);

            user.Password = _tokenService.GeneratePasswordHash(dto.NewPassword);
            await _context.SaveChangesAsync();

            await _emailService.SendChangedPasswordByAdmin(user.Email, $"{user.FirstName} {user.LastName}", dto.NewPassword);
        }

        public async Task CheckUserExistAsync(string email, string phoneNumber)
        {
            var existUser = await _context.AppUsers.Where(x => x.Email == email || x.PhoneNumber == phoneNumber)
            .Select(x => new
            {
                x.Email,
                x.PhoneNumber
            }).FirstOrDefaultAsync();

            if (existUser != null)
            {
                if (existUser.Email == email)
                    throw new BadRequestException("Email mövcuddur!");
                if (existUser.PhoneNumber == phoneNumber)
                    throw new BadRequestException("Telefon nömrəsi mövcuddur!");
            }
        }

        public async Task<List<UserDto>> GetAllUsersForPermissionAsync()
        {
            var allUsers = await _context.AppUsers
                //.Where(x => x.UserRole != UserRole.SuperAdmin)
                .ToListAsync();

            var users = allUsers.Select(user => new UserDto
            {
                UserId = user.Id,
                FullName = user.FirstName + " " + user.LastName,
                IsBlocked = user.IsBlock,
            }).ToList();

            return users;
        }

    }
}