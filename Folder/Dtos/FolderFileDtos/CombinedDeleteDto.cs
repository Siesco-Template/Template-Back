﻿namespace Folder.Dtos.FolderFileDtos
{
    public class CombinedDeleteDto
    {
        public List<string>? FolderPaths { get; set; } = [];
        public string? FolderPathForFiles { get; set; } = null!;
        public List<Guid>? FileIds { get; set; } = [];
    }

}
