using Auth.Business.Dtos.UserFileDtos;
using Auth.Business.Services;
using Auth.Core.Dtos.FolderFiles;
using Folder.Dtos.FolderDtos;
using Folder.Dtos.FolderFileDtos;
using Folder.Services.FolderFileServices;
using Folder.Services.FolderServices;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFilesController(IUserFileService _userFileService,
                                     IFolderService<UserFile> _folderService,
                                     IFolderFileService<UserFile> _folderFileService) : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> SyncUsersToFolder()
        {
            await _userFileService.SyncAllUsersToFolderAsync();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserFileDto dto)
        {
            return Ok(await _userFileService.CreateAsync(dto));
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



        /// <summary>
        /// Folder ve file-lari tek bir source-den silmek
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> BulkDeleteFoldersAndFiles([FromBody] CombinedDeleteDto dto)
        {
            if (dto.FolderPaths != null && dto.FolderPaths.Any())
            {
                await _userFileService.DeleteUsersFromFoldersAsync(dto.FolderPaths); // sql
                await _folderService.BulkDeleteFoldersAsync(dto.FolderPaths); // mongo
            }

            if (dto.FileIds != null && dto.FileIds.Any())
            {
                await _userFileService.BulkDeleteUsersAsync(dto.FileIds, dto.FolderPathForFiles);
            }

            return Ok();
        }


        /// <summary>
        /// Folder ve file-lari ferqli source-lerden silmek (Search sonrasi)
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteFromMultipleSources([FromBody] MultiSourceDeleteDto dto)
        {
            if (dto.FolderPathsToDelete != null && dto.FolderPathsToDelete.Any())
                await _userFileService.DeleteUsersFromFoldersAsync(dto.FolderPathsToDelete); // sql

            await _userFileService.DeleteFromMultipleSourcesAsync(dto); // mongo

            return Ok();
        }

    }
}
