using Auth.Business.Dtos.UserFileDtos;
using Auth.Core.Dtos.FolderFiles;
using Auth.Core.Entities;
using Auth.DAL.Contexts;
using Folder.Dtos.FolderDtos;
using Folder.Dtos.FolderFileDtos;
using Folder.Entities;
using Folder.HelperServices;
using Folder.Roots;
using Folder.Services.FolderFileServices;
using Folder.Services.FolderServices;
using Microsoft.EntityFrameworkCore;
using Template.Exceptions;

namespace Auth.Business.Services
{
    public class UserFileService(AuthDbContext _context,
                             IFolderService<UserFile> _folderService,
                             IFolderFileService<UserFile> _folderFileService) : IUserFileService
    {

        /// <summary>
        /// folder mentiqi sonradan qosulursa var olan file-lari cekmek ucun
        /// </summary>
        public async Task SyncAllUsersToFolderAsync()
        {
            var users = await _context.Users.ToListAsync();

            await _folderService.InitializeRootFolder();

            var mainFolder = await _folderService.GetFolderByPathAsync(RootFolders.Users);
            if (mainFolder == null)
                throw new BadRequestException("Əsas qovluq tapılmadı.");

            // 1. Əvvəlcə hazırda var olan max kodu tap
            var existingCodes = mainFolder.Files
                .OfType<BaseFile>()
                .Where(f => f.Code.StartsWith("usr"))
                .Select(f => f.Code)
                .ToList();

            int maxNumber = 0;
            foreach (var code in existingCodes)
            {
                var parts = code.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[1], out int number))
                {
                    if (number > maxNumber)
                        maxNumber = number;
                }
            }

            // 2. UserFile listini hazırla
            var userFiles = new List<UserFile>();
            foreach (var user in users)
            {
                maxNumber++;
                string code = $"usr_{maxNumber.ToString().PadLeft(3, '0')}";

                userFiles.Add(new UserFile
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    FileName = $"{user.FirstName}_{user.LastName}",
                    Code = code,
                    CreateDate = user.CreateDate
                });
            }

            // 3. Hamısını əlavə et
            await _folderFileService.AddFilesToFolderAsync(RootFolders.Users, userFiles);
        }

        public async Task<FileDto> CreateAsync(CreateUserFileDto dto)
        {
            string nextCode = await _folderFileService.GenerateNextFileCodeAsync(RootFolders.Users, "usr", 3); // usr_0004

            // emin ol ki, root folder var
            await _folderService.InitializeRootFolder();

            // 1. SQL-ə əlavə et
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userFile = new UserFile
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                FileName = $"{user.FirstName}_{user.LastName}",
                Code = nextCode,
                CreateDate = user.CreateDate,
            };

            await _folderFileService.AddFileToFolderAsync(dto.FolderPath, userFile);

            return new FileDto
            {
                Id = userFile.Id,
                FileName = userFile.FileName,
                CreateDate = userFile.CreateDate
            };
        }

        public async Task DeleteUsersAsync(Guid userId, string folderPath)
        {
            // 1. SQL-dən tap və sil
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new NotFoundException("İstifadəçi tapılmadı.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // 2. Mongo'dan tap və sil
            await _folderFileService.DeleteFilesAsync(folderPath, file => file.Id == userId);
        }

        public async Task BulkDeleteUsersAsync(List<Guid> userIds, string folderPath)
        {
            // 1. Bütün user-ləri bir query ilə tap
            var users = await _context.Users
                                       .Where(u => userIds.Contains(u.Id))
                                       .ToListAsync();

            if (!users.Any())
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
            var userIds = dto.FilesToDelete.Select(f => f.FileId).ToList();
            var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

            if (users.Any())
            {
                _context.Users.RemoveRange(users);
                await _context.SaveChangesAsync();
            }

            await _folderService.DeleteFromMultipleSourcesAsync(dto);
        }

        public async Task DeleteUsersFromFoldersAsync(List<string> folderPaths)
        {
            var root = await _folderService.GetFolderByPathAsync(RootFolders.Users);
            var userIdsToDelete = new HashSet<Guid>();

            foreach (var folderPath in folderPaths)
            {
                var folder = FolderTreeHelper.FindFolderRecursive(root, folderPath);
                if (folder?.Files != null)
                {
                    foreach (var file in folder.Files)
                        userIdsToDelete.Add(file.Id);
                }
            }

            if (userIdsToDelete.Any())
            {
                var users = await _context.Users.Where(u => userIdsToDelete.Contains(u.Id)).ToListAsync();
                if (users.Any())
                {
                    _context.Users.RemoveRange(users);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<FoldersAndFilesDto> SearchInFolderAsync(string path, string keyword)
        {
            var rootFolder = await _folderService.GetFolderByPathAsync(path);
            if (rootFolder == null)
                throw new NotFoundException("Qovluq tapılmadı.");

            var normalizedKeyword = Normalize(keyword);

            var matchingFolders = new List<FolderDto>();
            var matchingFiles = new List<FileDto>();

            searchRecursive(rootFolder, normalizedKeyword, matchingFolders, matchingFiles);

            return new FoldersAndFilesDto
            {
                Folders = matchingFolders,
                Files = matchingFiles
            };
        }

        private void searchRecursive(FolderEntity<UserFile> folder, string normalizedKeyword,
            List<FolderDto> matchingFolders,
            List<FileDto> matchingFiles)
        {
            if (!string.IsNullOrEmpty(folder.Name) && Normalize(folder.Name).Contains(normalizedKeyword))
            {
                matchingFolders.Add(new FolderDto
                {
                    Name = folder.Name,
                    Path = folder.Path,
                    Icon = folder.Icon,
                    CreateDate = folder.CreateDate
                });
            }

            foreach (var file in folder.Files)
            {
                if (Normalize(file.FullName).Contains(normalizedKeyword))
                {
                    matchingFiles.Add(new FileDto
                    {
                        Id = file.Id,
                        //FullName = file.FullName,
                        //Email = file.Email,
                        FileName = file.FileName,
                        CreateDate = file.CreateDate,
                        FolderPath = folder.Path
                    });
                }
            }

            foreach (var child in folder.Children)
            {
                searchRecursive(child, normalizedKeyword, matchingFolders, matchingFiles);
            }
        }

        private string Normalize(string input)
        {
            return input?.ToLowerInvariant()
                       .Replace(" ", "")
                       .Replace("ə", "e")
                       .Replace("ı", "i")
                       .Replace("ç", "c")
                       .Replace("ş", "s")
                       .Replace("ö", "o")
                       .Replace("ü", "u")
                       .Replace("ğ", "g")
                   ?? string.Empty;
        }
    }

    public interface IUserFileService
    {
        Task SyncAllUsersToFolderAsync();
        Task<FileDto> CreateAsync(CreateUserFileDto dto);
        Task DeleteUsersAsync(Guid userId, string folderPath);
        Task BulkDeleteUsersAsync(List<Guid> userIds, string folderPath);

        Task DeleteFromMultipleSourcesAsync(MultiSourceDeleteDto dto);

        Task DeleteUsersFromFoldersAsync(List<string> folderPaths);

        Task<FoldersAndFilesDto> SearchInFolderAsync(string path, string keyword);

    }
}