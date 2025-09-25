namespace Folder.Dtos.FolderFileDtos
{
    public class RenameFileDto
    {
        public string FolderPath { get; set; } = default!;
        public string FileId { get; set; }
        public string NewFileName { get; set; } = default!;
    }
}