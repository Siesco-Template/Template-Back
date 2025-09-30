using Folder.Dtos.FolderDtos;
using Folder.HelperServices;
using Folder.Roots;
using Folder.Services.FolderFileServices;
using Folder.Services.FolderServices;
using MainProject.API.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Exceptions;

namespace MainProject.API.Business.Services
{
    public class UserFileService(MainDbContext _context,
                             IFolderService _folderService,
                             IFolderFileService _folderFileService) : IUserFileService
    {
        public async Task DeleteUsersAsync(string userId, string folderPath)
        {
            var userGuid = Guid.Parse(userId);
            // 1. SQL-dən tap və sil
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userGuid) ?? throw new NotFoundException("İstifadəçi tapılmadı.");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // 2. Mongo'dan tap və sil
            await _folderFileService.DeleteFilesAsync(folderPath, file => file.Id == userId);
        }
            
        public async Task BulkDeleteUsersAsync(List<string> userIds, string folderPath)
        {
            var userGuids = userIds.Select(id => Guid.Parse(id)).ToList();
            // 1. Bütün user-ləri bir query ilə tap
            var users = await _context.Users
                                       .Where(u => userGuids.Contains(u.Id))
                                       .ToListAsync();

            if (users.Count == 0)
                throw new NotFoundException("İstifadəçi tapılmadı.");

            // 2. Bütün user-ləri bir dəfə Remove et
            _context.Users.RemoveRange(users);

            // 3. SaveChanges bir dəfə çağır
            await _context.SaveChangesAsync();

            // 4. Mongo-dan da bulk şəkildə sil
            await _folderFileService.DeleteFilesAsync(folderPath, file => userIds.Contains(file.Id));
        }

        public async Task DeleteFromMultipleSourcesAsync(MultiSourceDeleteDto dto)
        {
            List<string> userIds = dto.FilesToDelete.Select(f => f.FileId).ToList();
            var userGuids = userIds.Select(id => Guid.Parse(id)).ToList();
            var users = await _context.Users.Where(u => userGuids.Contains(u.Id)).ToListAsync();

            if (users.Count != 0)
            {
                _context.Users.RemoveRange(users);
                await _context.SaveChangesAsync();
            }

            await _folderService.DeleteFromMultipleSourcesAsync(dto);
        }

        public async Task DeleteUsersFromFoldersAsync(List<string> folderPaths)
        {
            var root = await _folderService.GetFolderByPathAsync(RootFolders.Root);
            var userIdsToDelete = new HashSet<string>();

            foreach (var folderPath in folderPaths)
            {
                var folder = FolderTreeHelper.FindFolderRecursive(root, folderPath);
                if (folder?.Files != null)
                {
                    foreach (var file in folder.Files)
                        userIdsToDelete.Add(file.SqlId);
                }
            }
            var userGuidsToDelete = userIdsToDelete.Select(id => Guid.Parse(id)).ToList();
            if (userIdsToDelete.Count != 0)
            {
                var users = await _context.Users.Where(u => userGuidsToDelete.Contains(u.Id)).ToListAsync();
                if (users.Count != 0)
                {
                    _context.Users.RemoveRange(users);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }

    public interface IUserFileService
    {
        Task DeleteUsersAsync(string userId, string folderPath);
        Task BulkDeleteUsersAsync(List<string> userIds, string folderPath);
        Task DeleteFromMultipleSourcesAsync(MultiSourceDeleteDto dto);
        Task DeleteUsersFromFoldersAsync(List<string> folderPaths);
    }
}