namespace Folder.Dtos.FolderFileDtos
{
    public class FileDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// search zamani path qayitsin deye elave etdim
        /// </summary>
        public string FolderPath { get; set; } = null!;
    }
}
