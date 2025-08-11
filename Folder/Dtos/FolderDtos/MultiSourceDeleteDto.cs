namespace Folder.Dtos.FolderDtos
{
    public class MultiSourceDeleteDto
    {
        public List<string>? FolderPathsToDelete { get; set; } = new();
        public List<FileDeleteItem>? FilesToDelete { get; set; } = new();
    }

    public class FileDeleteItem
    {
        public string FolderPath { get; set; } = null!;
        public Guid FileId { get; set; }
    }
}
