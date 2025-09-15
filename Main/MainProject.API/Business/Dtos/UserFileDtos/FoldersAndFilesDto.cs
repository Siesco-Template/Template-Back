using Folder.Dtos.FolderDtos;
using Folder.Dtos.FolderFileDtos;

namespace MainProject.API.Business.Dtos.UserFileDtos
{
    public class FoldersAndFilesDto
    {
        public List<FolderDto> Folders { get; set; } = [];
        public List<FileDto> Files { get; set; } = [];
    }
}