using Folder.Dtos.FolderDtos;
using Folder.Dtos.FolderFileDtos;
using Folder.Roots;
using Folder.Services.FolderFileServices;
using Folder.Services.FolderServices;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Exceptions;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController(IFolderService _folderService, IFolderFileService _folderFileService) : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderDto dto)
        {
            return Ok(await _folderService.CreateFolderAsync(dto.Name, dto.ParentPath, dto.Icon));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetOnlyFolders([FromQuery] string path = RootFolders.Root)
        {
            var folder = await _folderService.GetFolderByPathAsync(path)
                         ?? throw new NotFoundException("Folder not found");

            var folders = folder.Children
                .OrderByDescending(f => f.CreateDate)
                .Select(f => new FolderDto
                {
                    Name = f.Name,
                    Path = f.Path,
                    Icon = f.Icon,
                    CreateDate = f.CreateDate
                }).ToList();

            return Ok(folders);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetOnlyFiles([FromQuery] string path = RootFolders.Root)
        {
            var folder = await _folderService.GetFolderByPathAsync(path)
                         ?? throw new NotFoundException("Folder not found");

            var files = folder.Files
                .OrderByDescending(f => f.CreateDate)
                .Select(f => new FileDto
                {
                    Id = f.Id,
                    FileName = f.Name,
                    CreateDate = f.CreateDate,
                    FolderPath = folder.Path
                }).ToList();

            return Ok(files);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetFoldersAndFiles([FromQuery] string path)
        {
            if (path == "/") throw new BadRequestException();

            var folder = await _folderService.GetFolderByPathAsync(path)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            return Ok(new FoldersAndFilesDto
            {
                Folders = folder.Children
                    .OrderByDescending(f => f.CreateDate)
                    .Select(f => new FolderDto
                    {
                        Name = f.Name,
                        Path = f.Path,
                        Icon = f.Icon,
                        CreateDate = f.CreateDate
                    }).ToList(),

                Files = folder.Files
                    .OrderByDescending(f => f.CreateDate)
                    .Select(f => new FileDto
                    {
                        Id = f.Id,
                        FileName = f.Name,
                        CreateDate = f.CreateDate
                    }).ToList()
            });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetFolderDetail([FromQuery] string path)
        {
            var folder = await _folderService.GetFolderByPathAsync(path)
                         ?? throw new NotFoundException("Qovluq tapılmadı.");

            return Ok(new FolderDetailDto
            {
                Name = folder.Name,
                Path = folder.Path,
                Comment = folder.Comment,
                CreateDate = folder.CreateDate,
                UpdateDate = folder.UpdateDate
            });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> SearchInFolder([FromQuery] string? path, [FromQuery] string keyword)
        {
            path ??= RootFolders.Root;
            var result = await _folderService.SearchInFolderAsync(path, keyword);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RenameFolder([FromBody] RenameFolderDto dto)
        {
            await _folderService.RenameFolderAsync(dto.CurrentPath, dto.NewName);
            return Ok();
        }

        /// <summary>
        /// Folder ucun comment elave edir.(comment varsa kohneni silir yenini elave edir)
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto dto)
        {
            await _folderService.AddCommentAsync(dto.Path, dto.Comment);
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ChangeIcon([FromBody] ChangeIconDto dto)
        {
            await _folderService.ChangeIconAsync(dto.Path, dto.Icon);
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RenameFile([FromBody] RenameFileDto dto)
        {
            await _folderFileService.RenameFileAsync(dto.FolderPath, dto.FileId, dto.NewFileName);
            return Ok();
        }

        /// <summary>
        /// Folder ve file-lari tek bir source-den kopyalamaq
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> CopyFoldersAndFiles([FromBody] CombinedMoveDto dto)
        {
            if (dto.FolderNames != null && dto.FolderNames.Any())
            {
                await _folderService.BulkCopyFoldersAsync(dto.SourcePath, dto.TargetPath, dto.FolderNames);
            }

            if (dto.FileIds != null && dto.FileIds.Any())
            {
                await _folderFileService.CopyFilesAsync(
                dto.SourcePath,
                dto.TargetPath,
                file => dto.FileIds.Contains(file.Id)
              );
            }

            return Ok();
        }

        /// <summary>
        /// Folder ve file-lari ferqli source-lerden kopyalamaq (Search sonrasi)
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> CopyFromMultipleSources([FromBody] MultiSourceCopyMoveDto dto)
        {
            await _folderService.CopyFromMultipleSourcesAsync(dto);
            return Ok();
        }

        /// <summary>
        /// Folder ve file-lari tek bir source-den kocurmek
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> MoveFoldersAndFiles([FromBody] CombinedMoveDto dto)
        {
            if (dto.FolderNames != null && dto.FolderNames.Any())
            {
                await _folderService.BulkMoveFoldersAsync(dto.SourcePath, dto.TargetPath, dto.FolderNames);
            }

            if (dto.FileIds != null && dto.FileIds.Any())
            {
                await _folderFileService.MoveFilesAsync(
                dto.SourcePath,
                dto.TargetPath,
                file => dto.FileIds.Contains(file.Id)
               );
            }
            return Ok();
        }

        /// <summary>
        /// Folder ve file-lari ferqli source-lerden kocurmek (Search sonrasi)
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> MoveFromMultipleSources([FromBody] MultiSourceCopyMoveDto dto)
        {
            await _folderService.MoveFromMultipleSourcesAsync(dto);
            return Ok();
        }
    }
}