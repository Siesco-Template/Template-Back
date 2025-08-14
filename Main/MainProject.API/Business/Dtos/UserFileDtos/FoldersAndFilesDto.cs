using Folder.Dtos.FolderDtos;
using Folder.Dtos.FolderFileDtos;

namespace Auth.Business.Dtos.UserFileDtos
{
    public class FoldersAndFilesDto
    {
        public List<FolderDto> Folders { get; set; } = new();
        public List<FileDto> Files { get; set; } = new();
    }
}