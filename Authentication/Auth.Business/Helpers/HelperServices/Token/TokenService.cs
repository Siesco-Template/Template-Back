using Auth.Business.Models;
using Auth.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Business.Helpers.HelperServices.Token
{
    public class TokenService(IConfiguration _configuration)
    {
        public string CreateToken(AppUser user)
        {
            List<Claim> claims = new List<Claim>()
                    {
                       new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                       new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                       new Claim(ClaimTypes.Sid, user.Id.ToString()),
                       new Claim(ClaimTypes.Role, user.UserRole.ToString()),
                    };

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]!));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken jwtSecurity = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                DateTime.Now,
                DateTime.Now.AddMinutes(20000),
                //DateTime.Now.AddMinutes(expires),
                credentials);
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            string token = jwtHandler.WriteToken(jwtSecurity);
            return token;
        }

        public string CreatePasswordResetToken(AppUser user)
        {
            List<Claim> claims = new List<Claim>()
                    {
                       new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                       new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                       new Claim(ClaimTypes.Sid, user.Id.ToString()),
                       new Claim(ClaimTypes.Email, user.Email),
                    };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                DateTime.Now,
                DateTime.Now.AddHours(1),
                credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GeneratePasswordHash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes("siesco" + input + "auth");
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                var enc = Encoding.GetEncoding(0);
                byte[] buffer = enc.GetBytes(Convert.ToHexString(hashBytes));
                var sha1 = SHA1.Create();
                var hash = BitConverter.ToString(sha1.ComputeHash(buffer)).Replace("-", "");
                return hash;
            }
        }

        public RefreshToken GenerateRefreshToken(string token, int min)
        {
            var refreshToken = new RefreshToken
            {
                Token = GeneratePasswordHash(token),
                Created = DateTime.Now,
                Expires = DateTime.Now.AddMinutes(min),
            };

            return refreshToken;
        }
    }
}
