using Folder.Dtos.FolderDtos;
using Folder.Entities;

namespace Folder.Services.FolderServices
{
    public interface IFolderService<TFile> where TFile : BaseFile
    {
        Task<FolderEntity<TFile>?> GetFolderByPathAsync(string path);
        Task InitializeRootFolder();
        Task<FolderDto> CreateFolderAsync(string name, string parentPath, string? icon);

        Task RenameFolderAsync(string currentPath, string newName);

        Task BulkDeleteFoldersAsync(List<string> paths);
        Task DeleteFromMultipleSourcesAsync(MultiSourceDeleteDto dto);


        Task BulkCopyFoldersAsync(string sourceParentPath, string targetParentPath, List<string> folderNames);
        Task CopyFromMultipleSourcesAsync(MultiSourceCopyMoveDto dto);


        Task BulkMoveFoldersAsync(string sourceParentPath, string targetParentPath, List<string> folderNames);
        Task MoveFromMultipleSourcesAsync(MultiSourceCopyMoveDto dto);

        Task AddCommentAsync(string path, string comment);
        Task ChangeIconAsync(string path, string icon);
    }
}