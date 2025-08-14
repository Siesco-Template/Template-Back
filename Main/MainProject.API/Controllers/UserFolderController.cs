using Auth.Business.Dtos.UserFileDtos;
using Folder.Dtos.FolderDtos;
using Folder.Dtos.FolderFileDtos;
using Folder.Roots;
using Folder.Services.FolderServices;
using MainProject.API.Business.Dtos.FolderFiles;
using MainProject.API.Business.Services;
using Microsoft.AspNetCore.Mvc;
using Template.Exceptions;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFoldersController(
        IUserFileService _userFileService,
        IFolderService<UserFile> _folderService) : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderDto dto)
        {
            return Ok(await _folderService.CreateFolderAsync(dto.Name, dto.ParentPath, dto.Icon));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetOnlyFolders([FromQuery] string path = RootFolders.Users)
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
        public async Task<IActionResult> GetOnlyFiles([FromQuery] string path = RootFolders.Users)
        {
            var folder = await _folderService.GetFolderByPathAsync(path)
                         ?? throw new NotFoundException("Folder not found");

            var files = folder.Files
                .OrderByDescending(f => f.CreateDate)
                .Select(f => new FileDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    CreateDate = f.CreateDate
                }).ToList();

            return Ok(files);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetFoldersAndFiles([FromQuery] string path = RootFolders.Users)
        {
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
                        FileName = f.FileName,
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
            path ??= RootFolders.Users;
            var result = await _userFileService.SearchInFolderAsync(path, keyword);
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
    }
}