namespace SharedLibrary.Dtos
{
    public class DownloadItemDto
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string Extension { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}