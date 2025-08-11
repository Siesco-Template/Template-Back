using Auth.Business.Dtos.UserDtos;
using Auth.Core.Entities;
using Auth.DAL.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Auth.Business.Services
{
    public class UserService(AuthDbContext _context) : IUserService
    {
        public async Task<List<Auth.Business.Dtos.UserDtos.UserListDto>> GetAllAsync()
        {
            return await _context.Users.Select(u => new Auth.Business.Dtos.UserDtos.UserListDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
            }).ToListAsync();
        }

        public async Task<GetUserByIdDto?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new GetUserByIdDto
                {
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CreateDate = u.CreateDate,
                }).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Auth.Business.Dtos.UserDtos.CreateUserDto dto)
        {
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IsActive = true,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Auth.Business.Dtos.UserDtos.UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(dto.Id);

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public interface IUserService
    {
        Task<List<Auth.Business.Dtos.UserDtos.UserListDto>> GetAllAsync();
        Task<GetUserByIdDto?> GetByIdAsync(Guid id);
        Task CreateAsync(Auth.Business.Dtos.UserDtos.CreateUserDto dto);
        Task UpdateAsync(Auth.Business.Dtos.UserDtos.UpdateUserDto dto);
        Task DeleteAsync(Guid id);
    }
}