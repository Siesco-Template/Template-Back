using Folder.Abstractions;
using Folder.Dtos.FolderDtos;
using Folder.Entities;
using Folder.HelperServices;
using Folder.Utilities;
using MongoDB.Driver;
using System.Text.Json;
using Template.Exceptions;

namespace Folder.Services.FolderServices
{
    public class FolderService<TFile> : IFolderService<TFile> where TFile : BaseFile
    {
        private readonly IMongoCollection<FolderEntity<TFile>> _collection;
        private readonly string _rootPath;

        public FolderService(IFolderMongoContext context, string rootPath)
        {
            var collectionName = typeof(TFile).Name.ToLowerInvariant() + "Folder";
            _collection = context.GetCollection<TFile>(collectionName);
            _rootPath = rootPath;
        }

        public async Task<FolderEntity<TFile>?> GetFolderByPathAsync(string path)
        {
            if (path.TrimEnd('/') == _rootPath)
                return await GetRootFolderAsync();

            var rootFolder = await GetRootFolderAsync();
            return FolderTreeHelper.FindFolderRecursive(rootFolder, path);
        }

        public async Task InitializeRootFolder()
        {
            var exists = await _collection.Find(f => f.Path == _rootPath).AnyAsync();
            if (!exists)
            {
                var root = new FolderEntity<TFile>
                {
                    Name = _rootPath.Trim('/'),
                    Path = _rootPath,
                    Children = [],
                    Files = []
                };
                await _collection.InsertOneAsync(root);
            }
        }

        public async Task<FolderDto> CreateFolderAsync(string name, string parentPath, string? icon)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BadRequestException("Qovluq adı boş ola bilməz.");

            if (name.Contains("/"))
                throw new BadRequestException("Qovluq adında '/' simvolu istifadə edilə bilməz.");

            await InitializeRootFolder();
            var root = await GetRootFolderAsync();
            var parent = FolderTreeHelper.FindFolderRecursive(root, parentPath)
                         ?? throw new NotFoundException($"Qovluq tapılmadı.");

            // eyni adli folder varsa xeta atsin 
            if (parent.Children.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException($"'{name}' adlı qovluq artıq mövcuddur.");

            var newFolder = new FolderEntity<TFile>
            {
                Name = name,
                Path = parentPath.TrimEnd('/') + "/" + name,
                Children = new(),
                Files = new(),
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                Icon = icon
            };

            parent.Children.Add(newFolder);

            FolderTreeHelper.UpdateParentDates(root, parentPath);
            await SaveRootAsync(root);

            return new FolderDto
            {
                Name = newFolder.Name,
                Path = newFolder.Path,
                CreateDate = DateTime.UtcNow,
                Icon = icon
            };
        }

        public async Task RenameFolderAsync(string currentPath, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new BadRequestException(("Qovluq adı boş ola bilməz."));

            if (newName.Contains("/"))
                throw new BadRequestException("Qovluq adında '/' simvolu istifadə edilə bilməz.");

            var root = await GetRootFolderAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(root, currentPath)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");


            var oldPath = folder.Path;
            var parentPath = oldPath[..oldPath.LastIndexOf('/')];
            var newPath = parentPath + "/" + newName;

            // eyni adli folder varsa xeta atsin 
            var parentFolder = FolderTreeHelper.FindFolderRecursive(root, parentPath)
                                ?? throw new NotFoundException("Qovluq tapılmadı.");

            if (parentFolder.Children.Any(c => c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException($"'{newName}' adlı qovluq artıq mövcuddur.");
            folder.Name = newName;
            folder.Path = newPath;
            UpdateChildPaths(folder.Children, oldPath, newPath);

            FolderTreeHelper.UpdateParentDates(root, parentPath);
            await SaveRootAsync(root);
        }

        public async Task BulkDeleteFoldersAsync(List<string> paths)
        {
            var root = await GetRootFolderAsync();
            var set = new HashSet<string>(paths);
            RemoveFoldersRecursive(root, set);

            if (paths.Any())
            {
                var parentPath = paths.First()[..paths.First().LastIndexOf('/')];
                FolderTreeHelper.UpdateParentDates(root, parentPath);
            }

            await SaveRootAsync(root);
        }

        public async Task DeleteFromMultipleSourcesAsync(MultiSourceDeleteDto dto)
        {
            var root = await GetRootFolderAsync();

            if (dto.FolderPathsToDelete != null)
            {
                foreach (var folderPath in dto.FolderPathsToDelete)
                {
                    if (folderPath == _rootPath)
                        throw new BadRequestException("Əsas qovluq silinə bilməz.");

                    var parentPath = folderPath[..folderPath.LastIndexOf('/')];
                    var parent = FolderTreeHelper.FindFolderRecursive(root, parentPath)
                                 ?? throw new NotFoundException("Qovluq tapılmadı.");

                    parent.Children.RemoveAll(c => c.Path == folderPath);
                    FolderTreeHelper.UpdateParentDates(root, parentPath);
                }
            }

            if (dto.FilesToDelete != null)
            {
                foreach (var fileItem in dto.FilesToDelete)
                {
                    var folder = FolderTreeHelper.FindFolderRecursive(root, fileItem.FolderPath)
                                 ?? throw new NotFoundException("Qovluq tapılmadı.");

                    folder.Files = folder.Files.Where(f => f.Id != fileItem.FileId).ToList();
                    FolderTreeHelper.UpdateParentDates(root, fileItem.FolderPath);
                }
            }

            await SaveRootAsync(root);
        }

        public async Task BulkCopyFoldersAsync(string sourceParentPath, string targetParentPath, List<string> folderNames)
        {
            var rootFolder = await GetRootFolderAsync();
            var sourceParentFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, sourceParentPath)
                                  ?? throw new NotFoundException("Qovluq tapılmadı.");
            var targetParentFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, targetParentPath)
                                  ?? throw new NotFoundException("Qovluq tapılmadı.");

            // kopyaladigimiz yerde eyni adda folder varsa xeta atsin 
            var conflicting = folderNames.Intersect(targetParentFolder.Children.Select(c => c.Name)).ToList();
            if (conflicting.Any())
                throw new BadRequestException($"Hədəf qovluqda artıq bu qovluqlar mövcuddur: {string.Join(", ", conflicting)}");

            var foldersToCopy = sourceParentFolder.Children
                .Where(c => folderNames.Contains(c.Name))
                .ToList();

            if (!foldersToCopy.Any()) return;

            foreach (var folder in foldersToCopy)
            {
                var copiedFolder = FolderTreeHelper.DeepCloneWithNewPath(folder, sourceParentPath + "/" + folder.Name, targetParentPath + "/" + folder.Name);

                await RegenerateCodesRecursiveAsync(rootFolder, copiedFolder);

                targetParentFolder.Children.Add(copiedFolder);
            }

            FolderTreeHelper.UpdateParentDates(rootFolder, targetParentPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task CopyFromMultipleSourcesAsync(MultiSourceCopyMoveDto dto)
        {
            var rootFolder = await GetRootFolderAsync();

            var targetFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, dto.TargetPath)
                                ?? throw new NotFoundException("Qovluq tapılmadı.");

            // kopyaladigimiz yerde eyni adda file ve ya folder varsa xeta at
            // ps: optimallasdirmaq lazimdi 
            var folderConflicts = new List<string>();
            var fileConflicts = new List<string>();

            var existingFolderNames = targetFolder.Children.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var existingFileNames = targetFolder.Files?.Select(f => f.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase)
                                  ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (dto.FoldersToCopy != null)
            {
                foreach (var folderItem in dto.FoldersToCopy)
                {
                    if (existingFolderNames.Contains(folderItem.FolderName))
                        folderConflicts.Add(folderItem.FolderName);
                }
            }

            if (dto.FilesToCopy != null)
            {
                foreach (var fileItem in dto.FilesToCopy)
                {
                    var sourceFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, fileItem.SourcePath);
                    var file = sourceFolder?.Files.FirstOrDefault(f => f.Id == fileItem.FileId);
                    if (file != null && existingFileNames.Contains(file.FileName))
                        fileConflicts.Add(file.FileName);
                }
            }

            if (folderConflicts.Any() || fileConflicts.Any())
            {
                var errorMsg = "Ad uyğunluğu səbəbilə köçürmə ləğv edildi:\n";
                if (folderConflicts.Any())
                    errorMsg += $"Qovluqlar: {string.Join(", ", folderConflicts)}\n";
                if (fileConflicts.Any())
                    errorMsg += $"Fayllar: {string.Join(", ", fileConflicts)}";

                throw new BadRequestException(errorMsg);
            }

            if (dto.FoldersToCopy != null)
            {
                foreach (var folderItem in dto.FoldersToCopy)
                {
                    var sourceParent = FolderTreeHelper.FindFolderRecursive(rootFolder, folderItem.SourcePath)
                                       ?? throw new NotFoundException("Qovluq tapılmadı.");

                    var folderToCopy = sourceParent.Children.FirstOrDefault(c => c.Name == folderItem.FolderName)
                                       ?? throw new NotFoundException("Qovluq tapılmadı.");

                    var copied = FolderTreeHelper.DeepCloneWithNewPath(
                        folderToCopy,
                        folderItem.SourcePath + "/" + folderItem.FolderName,
                        dto.TargetPath + "/" + folderItem.FolderName);

                    await RegenerateCodesRecursiveAsync(rootFolder, copied);

                    targetFolder.Children.Add(copied);
                }
            }

            if (dto.FilesToCopy != null)
            {
                foreach (var fileItem in dto.FilesToCopy)
                {
                    var sourceFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, fileItem.SourcePath)
                                        ?? throw new NotFoundException("Qovluq tapılmadı.");

                    var fileToCopy = sourceFolder.Files.FirstOrDefault(f => f.Id == fileItem.FileId)
                                     ?? throw new NotFoundException("Qovluq tapılmadı.");

                    var cloned = JsonSerializer.Deserialize<TFile>(JsonSerializer.Serialize(fileToCopy))!;

                    if (cloned is BaseFile baseFile)
                    {
                        var (prefix, numberLength) = CodeParsingHelper.ExtractPrefixAndLength(baseFile.Code);
                        var existingCodes = FolderTreeHelper.GetAllCodesWithPrefix(rootFolder, prefix);
                        baseFile.Code = FileCodeGenerator.GenerateNextCode(existingCodes, prefix, numberLength);
                    }

                    targetFolder.Files ??= new List<TFile>();
                    targetFolder.Files.Add(cloned);
                }
            }

            targetFolder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(rootFolder, dto.TargetPath);
            await SaveRootAsync(rootFolder);
        }

        public async Task BulkMoveFoldersAsync(string sourceParentPath, string targetParentPath, List<string> folderNames)
        {
            var root = await GetRootFolderAsync();
            var sourceParent = FolderTreeHelper.FindFolderRecursive(root, sourceParentPath)
                              ?? throw new NotFoundException("Hədəf qovluq tapılmadı.");
            var targetParent = FolderTreeHelper.FindFolderRecursive(root, targetParentPath)
                              ?? throw new NotFoundException("Mənbə qovluq tapılmadı.");

            var conflicting = folderNames.Intersect(targetParent.Children.Select(c => c.Name)).ToList();
            if (conflicting.Any())
                throw new BadRequestException($"Hədəf qovluqda artıq bu qovluqlar mövcuddur: {string.Join(", ", conflicting)}");
            var foldersToMove = sourceParent.Children
                .Where(c => folderNames.Contains(c.Name))
                .ToList();

            foreach (var folder in foldersToMove)
            {
                var sourcePath = $"{sourceParentPath}/{folder.Name}";
                var targetPath = $"{targetParentPath}/{folder.Name}";

                var moved = FolderTreeHelper.DeepCloneWithNewPath(folder, sourcePath, targetPath);

                targetParent.Children.Add(moved);
            }

            sourceParent.Children.RemoveAll(c => folderNames.Contains(c.Name));

            FolderTreeHelper.UpdateParentDates(root, sourceParentPath);
            FolderTreeHelper.UpdateParentDates(root, targetParentPath);
            await SaveRootAsync(root);
        }

        public async Task MoveFromMultipleSourcesAsync(MultiSourceCopyMoveDto dto)
        {
            var rootFolder = await GetRootFolderAsync();

            var targetFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, dto.TargetPath)
                                ?? throw new NotFoundException("Qovluq tapılmadı.");

            // kocurduyumuz yerde eyni adda data varsa xeta atiriq
            var folderConflicts = new List<string>();
            var fileConflicts = new List<string>();

            var existingFolderNames = targetFolder.Children.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var existingFileNames = targetFolder.Files?.Select(f => f.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase)
                                  ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (dto.FoldersToCopy != null)
            {
                foreach (var folderItem in dto.FoldersToCopy)
                {
                    if (existingFolderNames.Contains(folderItem.FolderName))
                        folderConflicts.Add(folderItem.FolderName);
                }
            }

            if (dto.FilesToCopy != null)
            {
                foreach (var fileItem in dto.FilesToCopy)
                {
                    var sourceFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, fileItem.SourcePath);
                    var file = sourceFolder?.Files.FirstOrDefault(f => f.Id == fileItem.FileId);
                    if (file != null && existingFileNames.Contains(file.FileName))
                        fileConflicts.Add(file.FileName);
                }
            }

            if (folderConflicts.Any() || fileConflicts.Any())
            {
                var errorMsg = "Ad uyğunluğu səbəbilə köçürmə ləğv edildi:\n";
                if (folderConflicts.Any())
                    errorMsg += $"Qovluqlar: {string.Join(", ", folderConflicts)}\n";
                if (fileConflicts.Any())
                    errorMsg += $"Fayllar: {string.Join(", ", fileConflicts)}";
                throw new BadRequestException(errorMsg);
            }

            if (dto.FoldersToCopy != null)
            {
                foreach (var folderItem in dto.FoldersToCopy)
                {
                    var sourceParent = FolderTreeHelper.FindFolderRecursive(rootFolder, folderItem.SourcePath)
                                       ?? throw new NotFoundException("Qovluq tapılmadı.");

                    var folderToMove = sourceParent.Children.FirstOrDefault(c => c.Name == folderItem.FolderName)
                                       ?? throw new NotFoundException("Qovluq tapılmadı.");

                    var sourcePath = $"{folderItem.SourcePath}/{folderItem.FolderName}";
                    var targetPath = $"{dto.TargetPath}/{folderItem.FolderName}";

                    var moved = FolderTreeHelper.DeepCloneWithNewPath(folderToMove, sourcePath, targetPath);

                    sourceParent.Children.Remove(folderToMove);
                    targetFolder.Children.Add(moved);
                }
            }

            if (dto.FilesToCopy != null)
            {
                foreach (var fileItem in dto.FilesToCopy)
                {
                    var sourceFolder = FolderTreeHelper.FindFolderRecursive(rootFolder, fileItem.SourcePath)
                                        ?? throw new NotFoundException("Qovluq tapılmadı.");

                    var fileToMove = sourceFolder.Files.FirstOrDefault(f => f.Id == fileItem.FileId)
                                     ?? throw new NotFoundException("Qovluq tapılmadı.");

                    sourceFolder.Files.Remove(fileToMove);

                    targetFolder.Files ??= new List<TFile>();
                    targetFolder.Files.Add(fileToMove);
                }
            }

            targetFolder.UpdateDate = DateTime.UtcNow;


            foreach (var folderItem in dto.FoldersToCopy)
                FolderTreeHelper.UpdateParentDates(rootFolder, folderItem.SourcePath);

            foreach (var fileItem in dto.FilesToCopy)
                FolderTreeHelper.UpdateParentDates(rootFolder, fileItem.SourcePath);

            FolderTreeHelper.UpdateParentDates(rootFolder, dto.TargetPath);

            await SaveRootAsync(rootFolder);
        }

        public async Task AddCommentAsync(string path, string? comment)
        {
            var root = await GetRootFolderAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(root, path)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            folder.Comment = comment?.Trim();
            folder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(root, path);
            await SaveRootAsync(root);
        }

        public async Task ChangeIconAsync(string path, string icon)
        {
            var root = await GetRootFolderAsync();
            var folder = FolderTreeHelper.FindFolderRecursive(root, path)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            folder.Icon = icon;
            folder.UpdateDate = DateTime.UtcNow;

            FolderTreeHelper.UpdateParentDates(root, path);
            await SaveRootAsync(root);
        }


        private async Task RegenerateCodesRecursiveAsync(FolderEntity<TFile> rootFolder, FolderEntity<TFile> folderToProcess)
        {
            foreach (var file in folderToProcess.Files.OfType<BaseFile>())
            {
                var (prefix, numberLength) = CodeParsingHelper.ExtractPrefixAndLength(file.Code);
                var existingCodes = FolderTreeHelper.GetAllCodesWithPrefix(rootFolder, prefix);
                file.Code = FileCodeGenerator.GenerateNextCode(existingCodes, prefix, numberLength);
            }

            foreach (var child in folderToProcess.Children)
            {
                await RegenerateCodesRecursiveAsync(rootFolder, child);
            }
        }


        private void UpdateChildPaths(List<FolderEntity<TFile>> children, string oldBasePath, string newBasePath)
        {
            foreach (var child in children)
            {
                child.Path = child.Path.Replace(oldBasePath, newBasePath);
                UpdateChildPaths(child.Children, oldBasePath, newBasePath);
            }
        }

        private void RemoveFoldersRecursive(FolderEntity<TFile> folder, HashSet<string> pathsToDelete)
        {
            folder.Children.RemoveAll(c => pathsToDelete.Contains(c.Path));
            foreach (var child in folder.Children)
                RemoveFoldersRecursive(child, pathsToDelete);

            folder.UpdateDate = DateTime.UtcNow;
        }

        private async Task<FolderEntity<TFile>> GetRootFolderAsync()
        {
            return await _collection.Find(f => f.Path == _rootPath).FirstOrDefaultAsync()
                   ?? throw new NotFoundException("Əsas qovluq tapılmadı.");
        }

        private async Task SaveRootAsync(FolderEntity<TFile> rootFolder)
        {
            var filter = Builders<FolderEntity<TFile>>.Filter.Eq(f => f.Path, _rootPath);
            await _collection.ReplaceOneAsync(filter, rootFolder);
        }
    }
}