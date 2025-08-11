namespace Folder.Dtos.FolderDtos
{
    /// <summary>
    /// search sonrasi combine copy ucun 
    /// </summary>
    public class MultiSourceCopyMoveDto
    {
        public string TargetPath { get; set; } = null!;

        public List<FolderCopyItem>? FoldersToCopy { get; set; } = new();
        public List<FileCopyItem>? FilesToCopy { get; set; } = new();
    }

    public class FolderCopyItem
    {
        public string SourcePath { get; set; } = null!;
        public string FolderName { get; set; } = null!;
    }

    public class FileCopyItem
    {
        public string SourcePath { get; set; } = null!;
        public Guid FileId { get; set; }
    }
}
