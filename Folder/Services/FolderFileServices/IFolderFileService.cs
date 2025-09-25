using Folder.Entities;

namespace Folder.Services.FolderFileServices
{
    public interface IFolderFileService
    {
        Task AddFileToFolderAsync(string folderPath, BaseFile file);
        Task AddFilesToFolderAsync(string folderPath, List<BaseFile> files);
        Task RenameFileAsync(string folderPath, string sqlId, string newName);
        Task DeleteFilesAsync(string folderPath, Func<BaseFile, bool> predicate);
        Task CopyFilesAsync(string sourceFolderPath, string targetFolderPath, Func<BaseFile, bool> predicate);
        Task MoveFilesAsync(string sourceFolderPath, string targetFolderPath, Func<BaseFile, bool> predicate);
        Task DeleteFileAsync(string folderPath, string fileId);
        Task<BaseFile> CreateFileAsync(string folderPath, Guid sqlId, string name);
    }
}