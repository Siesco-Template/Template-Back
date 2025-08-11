using Folder.Abstractions;
using Folder.Entities;
using Folder.HelperServices;
using Folder.Services.FolderServices;
using Folder.Utilities;
using MongoDB.Driver;
using System.Text.Json;
using Template.Exceptions;

namespace Folder.Services.FolderFileServices
{
    public class FolderFileService<TFile> : IFolderFileService<TFile> where TFile : BaseFile
    {
        private readonly IMongoCollection<FolderEntity<TFile>> _collection;
        private readonly IFolderService<TFile> _folderService;
        private readonly string _rootPath;

        public FolderFileService(IFolderMongoContext context, IFolderService<TFile> folderService, string rootPath)
        {
            var collectionName = typeof(TFile).Name.ToLowerInvariant() + "Folder";
            _collection = context.GetCollection<TFile>(collectionName);
            _folderService = folderService;
            _rootPath = rootPath;
        }

        public async Task AddFileToFolderAsync(string folderPath, TFile file)
        {
            var rootFolder = await GetRootAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(rootFolder, folderPath)
                         ?? throw new NotFoundException($"'{folderPath}' yolu üzrə qovluq tapılmadı.");

            folder.Files ??= new List<TFile>();

            // ✅ Aynı isimde dosya varsa hata at
            if (folder.Files.Any(f => f.FileName.Equals(file.FileName, StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException($"'{file.FileName}' adlı fayl artıq mövcuddur.");

            folder.Files.Add(file);
            folder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task AddFilesToFolderAsync(string folderPath, List<TFile> files)
        {
            var rootFolder = await GetRootAsync();
            var folder = await _folderService.GetFolderByPathAsync(folderPath)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            folder.Files ??= new List<TFile>();
            folder.Files.AddRange(files);
            folder.UpdateDate = DateTime.UtcNow;

            var filter = Builders<FolderEntity<TFile>>.Filter.Eq(f => f.Path, folderPath);
            var update = Builders<FolderEntity<TFile>>.Update.Set(f => f.Files, folder.Files);

            await _collection.UpdateOneAsync(filter, update);
            FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);

            await SaveRootAsync(rootFolder);
        }

        public async Task RenameFileAsync(string folderPath, Guid fileId, string newFileName)
        {
            var rootFolder = await GetRootAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(rootFolder, folderPath)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            var file = folder.Files.FirstOrDefault(f => f.Id == fileId)
                       ?? throw new NotFoundException("Fayl tapılmadı.");

            if (folder.Files.Any(f => f.Id != fileId && f.FileName.Equals(newFileName, StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException($"'{newFileName}' adlı fayl artıq mövcuddur.");

            file.FileName = newFileName;
            folder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task CopyFilesAsync(string sourceFolderPath, string targetFolderPath, Func<TFile, bool> predicate)
        {
            var rootFolder = await GetRootAsync();
            var sourceFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, sourceFolderPath)
                                ?? throw new NotFoundException("Mənbə qovluq tapılmadı.");
            var targetFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, targetFolderPath)
                                ?? throw new NotFoundException("Hədəf qovluq tapılmadı.");

            var filesToCopy = sourceFolder.Files.Where(predicate).ToList();
            if (!filesToCopy.Any())
                throw new NotFoundException("Kopyalanacaq uyğun fayl tapılmadı.");

            targetFolder.Files ??= new List<TFile>();

            // kopyaladigimiz yerde eyni adli file varsa xeta atsin
            var existingFileNames = targetFolder.Files.Select(f => f.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var conflictedFiles = filesToCopy.Where(f => existingFileNames.Contains(f.FileName)).ToList();
            if (conflictedFiles.Any())
                throw new BadRequestException($"Hədəf qovluqda artıq bu fayllar mövcuddur: {string.Join(", ", conflictedFiles.Select(f => f.FileName))}");


            foreach (var file in filesToCopy)
            {
                var clonedFile = CloneFile(file);

                if (clonedFile is BaseFile baseFile)
                {
                    var (prefix, numberLength) = CodeParsingHelper.ExtractPrefixAndLength(baseFile.Code);
                    var existingCodes = FolderTreeHelper.GetAllCodesWithPrefix(rootFolder, prefix);
                    baseFile.Code = FileCodeGenerator.GenerateNextCode(existingCodes, prefix, numberLength);
                }

                targetFolder.Files.Add(clonedFile);
            }

            targetFolder.UpdateDate = DateTime.UtcNow;
            FolderTreeHelper.UpdateParentDates(rootFolder, targetFolderPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task MoveFilesAsync(string sourceFolderPath, string targetFolderPath, Func<TFile, bool> predicate)
        {
            var rootFolder = await GetRootAsync();
            var sourceFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, sourceFolderPath)
                               ?? throw new NotFoundException("Mənbə qovluq tapılmadı.");
            var targetFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, targetFolderPath)
                               ?? throw new NotFoundException("Hədəf qovluq tapılmadı.");

            var filesToMove = sourceFolder.Files.Where(predicate).ToList();
            if (!filesToMove.Any())
                throw new NotFoundException("Uyğun fayl tapılmadı.");

            targetFolder.Files ??= new List<TFile>();

            // kocurduyumuz yerde eyni adda folder varsa xeta atsin 
            var targetFileNames = targetFolder.Files.Select(f => f.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var conflicting = filesToMove.Where(f => targetFileNames.Contains(f.FileName)).ToList();
            if (conflicting.Any())
                throw new BadRequestException($"Hədəf qovluqda artıq bu fayllar mövcuddur: {string.Join(", ", conflicting.Select(f => f.FileName))}");

            targetFolder.Files.AddRange(filesToMove);

            sourceFolder.Files = sourceFolder.Files.Where(file => !predicate(file)).ToList();
            sourceFolder.UpdateDate = DateTime.UtcNow;
            targetFolder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(rootFolder, sourceFolderPath);
            FolderTreeHelper.UpdateParentDates(rootFolder, targetFolderPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task DeleteFilesAsync(string folderPath, Func<TFile, bool> predicate)
        {
            var rootFolder = await GetRootAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(rootFolder, folderPath)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            folder.Files = folder.Files.Where(file => !predicate(file)).ToList();
            folder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(rootFolder, folderPath);
            await SaveRootAsync(rootFolder);
        }


        public async Task<string> GenerateNextFileCodeAsync(string folderPath, string prefix, int numberLength)
        {
            var folder = await _folderService.GetFolderByPathAsync(folderPath)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            var matchingCodes = folder.Files
                .OfType<BaseFile>()
                .Where(f => f.Code.StartsWith(prefix))
                .Select(f => f.Code)
                .ToList();

            return FileCodeGenerator.GenerateNextCode(matchingCodes, prefix, numberLength);
        }

        private TFile CloneFile(TFile file)
        {
            var serialized = JsonSerializer.Serialize(file);
            return JsonSerializer.Deserialize<TFile>(serialized)!;
        }

        private async Task<FolderEntity<TFile>> GetRootAsync()
        {
            return await _collection.Find(f => f.Path == _rootPath).FirstOrDefaultAsync()
                   ?? throw new NotFoundException("Qovluq tapılmadı.");
        }

        private async Task SaveRootAsync(FolderEntity<TFile> rootFolder)
        {
            var filter = Builders<FolderEntity<TFile>>.Filter.Eq(f => f.Path, _rootPath);
            await _collection.ReplaceOneAsync(filter, rootFolder);
        }
    }
}