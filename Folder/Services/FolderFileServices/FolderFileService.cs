using Folder.Entities;
using Folder.HelperServices;
using Folder.Services.FolderServices;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedLibrary.Exceptions;
using SharedLibrary.HelperServices;
using System.Text.Json;

namespace Folder.Services.FolderFileServices
{
    public class FolderFileService : IFolderFileService
    {
        private readonly IMongoCollection<FolderEntity> _collection;
        private readonly IFolderService _folderService;
        private readonly string _rootPath = "/";
        public FolderFileService(MongoDbService mongoDbService, IFolderService folderService)
        {
            var database = mongoDbService.GetDatabase();
            _collection = database.GetCollection<FolderEntity>("Folders");
            _folderService = folderService;
        }

        public async Task AddFileToFolderAsync(string folderPath, BaseFile file)
        {
            var rootFolder = await GetRootAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(rootFolder, folderPath)
                         ?? throw new NotFoundException($"'{folderPath}' yolu üzrə qovluq tapılmadı.");

            folder.Files ??= [];

            if (folder.Files.Any(f => f.Name.Equals(file.Name, StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException($"'{file.Name}' adlı fayl artıq mövcuddur.");

            folder.Files.Add(file);
            folder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task AddFilesToFolderAsync(string folderPath, List<BaseFile> files)
        {
            var rootFolder = await GetRootAsync();
            var folder = await _folderService.GetFolderByPathAsync(folderPath)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            folder.Files ??= [];
            folder.Files.AddRange(files);
            folder.UpdateDate = DateTime.UtcNow;

            var filter = Builders<FolderEntity>.Filter.Eq(f => f.Path, folderPath);
            var update = Builders<FolderEntity>.Update.Set(f => f.Files, folder.Files);

            await _collection.UpdateOneAsync(filter, update);
            FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);

            await SaveRootAsync(rootFolder);
        }

        public async Task RenameFileAsync(string folderPath, string sqlId, string newName)
        {
            var rootFolder = await GetRootAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(rootFolder, folderPath)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            var file = folder.Files.FirstOrDefault(f => f.SqlId == sqlId)
                       ?? throw new NotFoundException("Fayl tapılmadı.");

            if (folder.Files.Any(f => f.SqlId != sqlId && f.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException($"'{newName}' adlı fayl artıq mövcuddur.");

            file.Name = newName;
            folder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task DeleteFilesAsync(string folderPath, Func<BaseFile, bool> predicate)
        {
            var rootFolder = await GetRootAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(rootFolder, folderPath)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            folder.Files = folder.Files.Where(file => !predicate(file)).ToList();
            folder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task DeleteFileAsync(string folderPath, string fileId)
        {
            var rootFolder = await GetRootAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(rootFolder, folderPath)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            var fileToRemove = folder.Files.FirstOrDefault(f => f.SqlId == fileId) ?? throw new NotFoundException("Fayl tapılmadı.");
            folder.Files.Remove(fileToRemove);
            folder.UpdateDate = DateTime.UtcNow;

            if (folderPath != "/")
                FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);

            await SaveRootAsync(rootFolder);
        }

        public async Task CopyFilesAsync(string sourceFolderPath, string targetFolderPath, Func<BaseFile, bool> predicate)
        {
            var rootFolder = await GetRootAsync();
            var sourceFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, sourceFolderPath)
                                ?? throw new NotFoundException("Mənbə qovluq tapılmadı.");
            var targetFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, targetFolderPath)
                                ?? throw new NotFoundException("Hədəf qovluq tapılmadı.");

            var filesToCopy = sourceFolder.Files.Where(predicate).ToList();
            if (!filesToCopy.Any())
                throw new NotFoundException("Kopyalanacaq uyğun fayl tapılmadı.");

            targetFolder.Files ??= [];

            var existingFileNames = targetFolder.Files.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var conflictedFiles = filesToCopy.Where(f => existingFileNames.Contains(f.Name)).ToList();
            if (conflictedFiles.Count != 0)
                throw new BadRequestException($"Hədəf qovluqda artıq bu fayllar mövcuddur: {string.Join(", ", conflictedFiles.Select(f => f.Name))}");

            foreach (var file in filesToCopy)
            {
                var clonedFile = CloneFile(file);

                //var (prefix, numberLength) = CodeParsingHelper.ExtractPrefixAndLength(clonedFile.Code);
                //var existingCodes = FolderTreeHelper.GetAllCodesWithPrefix(rootFolder, prefix);
                //clonedFile.Code = FileCodeGenerator.GenerateNextCode(existingCodes, prefix, numberLength);

                targetFolder.Files.Add(clonedFile);
            }

            targetFolder.UpdateDate = DateTime.UtcNow;
            FolderTreeHelper.UpdateParentDates(rootFolder, targetFolderPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task MoveFilesAsync(string sourceFolderPath, string targetFolderPath, Func<BaseFile, bool> predicate)
        {
            var rootFolder = await GetRootAsync();
            var sourceFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, sourceFolderPath)
                               ?? throw new NotFoundException("Mənbə qovluq tapılmadı.");
            var targetFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, targetFolderPath)
                               ?? throw new NotFoundException("Hədəf qovluq tapılmadı.");

            var filesToMove = sourceFolder.Files.Where(predicate).ToList();
            if (!filesToMove.Any())
                throw new NotFoundException("Uyğun fayl tapılmadı.");

            targetFolder.Files ??= [];

            var targetFileNames = targetFolder.Files.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var conflicting = filesToMove.Where(f => targetFileNames.Contains(f.Name)).ToList();
            if (conflicting.Any())
                throw new BadRequestException($"Hədəf qovluqda artıq bu fayllar mövcuddur: {string.Join(", ", conflicting.Select(f => f.Name))}");

            targetFolder.Files.AddRange(filesToMove);

            sourceFolder.Files = sourceFolder.Files.Where(file => !predicate(file)).ToList();
            sourceFolder.UpdateDate = DateTime.UtcNow;
            targetFolder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(rootFolder, sourceFolderPath);
            FolderTreeHelper.UpdateParentDates(rootFolder, targetFolderPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task<BaseFile> CreateFileAsync(string folderPath, Guid sqlId, string name)
        {
            var rootFolder = await GetRootAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(rootFolder, folderPath)
                         ?? throw new NotFoundException($"'{folderPath}' yolu üzrə qovluq tapılmadı.");

            folder.Files ??= [];

            // Eyni adda file varsa xeta
            if (folder.Files.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException($"'{name}' adlı fayl artıq mövcuddur.");

            var newFile = new BaseFile
            {
                Id = ObjectId.GenerateNewId().ToString(),
                SqlId = sqlId.ToString(),
                Name = name,
                CreateDate = DateTime.UtcNow
            };

            folder.Files.Add(newFile);
            folder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);
            await SaveRootAsync(rootFolder);

            return newFile;
        }

        public async Task BulkDeleteFileAsync(string folderPath, List<string> fileIds)
        {
            if (fileIds == null || fileIds.Count == 0)
                throw new BadRequestException("Silinəcək fayl id-ləri göndərilməyib.");

            var rootFolder = await GetRootAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(rootFolder, folderPath)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            var filesToRemove = folder.Files.Where(f => fileIds.Contains(f.SqlId)).ToList();

            if (filesToRemove.Count == 0)
                throw new NotFoundException("Silinəcək fayl tapılmadı.");

            foreach (var file in filesToRemove)
            {
                folder.Files.Remove(file);
            }

            folder.UpdateDate = DateTime.UtcNow;
            FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);
            await SaveRootAsync(rootFolder);
        }

        private BaseFile CloneFile(BaseFile file)
        {
            var serialized = JsonSerializer.Serialize(file);
            return JsonSerializer.Deserialize<BaseFile>(serialized)!;
        }

        private async Task<FolderEntity> GetRootAsync()
        {
            return await _collection.Find(f => f.Path == _rootPath).FirstOrDefaultAsync()
                   ?? throw new NotFoundException("Qovluq tapılmadı.");
        }

        private async Task SaveRootAsync(FolderEntity rootFolder)
        {
            var filter = Builders<FolderEntity>.Filter.Eq(f => f.Path, _rootPath);
            await _collection.ReplaceOneAsync(filter, rootFolder);
        }

    }
}