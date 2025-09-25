namespace Folder.Dtos.FolderDtos
{
    public class MultiSourceDeleteDto
    {
        public List<string>? FolderPathsToDelete { get; set; } = [];
        public List<FileDeleteItem>? FilesToDelete { get; set; } = [];
    }

    public class FileDeleteItem
    {
        public string FolderPath { get; set; } = null!;
        public string FileId { get; set; }
    }
}