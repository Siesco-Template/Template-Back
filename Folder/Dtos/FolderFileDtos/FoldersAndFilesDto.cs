using Folder.Dtos.FolderDtos;

namespace Folder.Dtos.FolderFileDtos
{
    public class FoldersAndFilesDto
    {
        public List<FolderDto> Folders { get; set; } = [];
        public List<FileDto> Files { get; set; } = [];
    }
}