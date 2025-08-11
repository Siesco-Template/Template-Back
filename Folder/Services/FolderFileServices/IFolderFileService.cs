using Folder.Entities;

namespace Folder.Services.FolderFileServices
{
    public interface IFolderFileService<TFile> where TFile : BaseFile
    {
        Task AddFileToFolderAsync(string folderPath, TFile file);
        Task AddFilesToFolderAsync(string folderPath, List<TFile> file);
        Task RenameFileAsync(string folderPath, Guid fileId, string newFileName);
        Task DeleteFilesAsync(string folderPath, Func<TFile, bool> predicate);
        Task CopyFilesAsync(string sourceFolderPath, string targetFolderPath, Func<TFile, bool> predicate);
        Task MoveFilesAsync(string sourceFolderPath, string targetFolderPath, Func<TFile, bool> predicate);
        Task<string> GenerateNextFileCodeAsync(string folderPath, string prefix, int numberLength);
    }
}