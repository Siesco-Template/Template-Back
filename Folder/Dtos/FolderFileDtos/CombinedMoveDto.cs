namespace Folder.Dtos.FolderFileDtos
{
    public class CombinedMoveDto
    {
        public string SourcePath { get; set; } = null!;
        public string TargetPath { get; set; } = null!;
        public List<string>? FolderNames { get; set; } = [];
        public List<Guid>? FileIds { get; set; } = [];
    }

}
