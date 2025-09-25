using Folder.Services.FolderFileServices;
using MainProject.API.Business.Dtos.UserDtos;
using MainProject.API.DAL.Contexts;
using MainProject.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Exceptions;

namespace MainProject.API.Business.Services
{
    public class UserService(MainDbContext _context, IFolderFileService _folderFileService) : IUserService
    {
        public async Task<List<UserListDto>> GetAllAsync()
        {
            return await _context.Users.Select(u => new UserListDto
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

        public async Task CreateAsync(CreateUserDto dto)
        {
            var user = new FolderUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IsActive = true,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _folderFileService.CreateFileAsync(

                   folderPath: "/Users",
                   sqlId: user.Id,
                   name: $"{user.FirstName}_{user.LastName}"
            );
        }

        public async Task UpdateAsync(UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(dto.Id) ?? throw new NotFoundException();

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;

            await _context.SaveChangesAsync();

            var newFileName = $"{dto.FirstName}_{dto.LastName}";

            var userId = user.Id.ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                var folderPath = "/Users";
                await _folderFileService.RenameFileAsync(folderPath, userId, newFileName);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var folderPath = "/Users";
            var fileId = user.Id.ToString();

            if (!string.IsNullOrEmpty(fileId))
            {
                await _folderFileService.DeleteFileAsync(folderPath, fileId);
            }
        }
    }

    public interface IUserService
    {
        Task<List<UserListDto>> GetAllAsync();
        Task<GetUserByIdDto?> GetByIdAsync(Guid id);
        Task CreateAsync(CreateUserDto dto);
        Task UpdateAsync(UpdateUserDto dto);
        Task DeleteAsync(Guid id);
    }
}