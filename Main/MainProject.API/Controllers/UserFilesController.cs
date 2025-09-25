using Folder.Dtos.FolderDtos;
using Folder.Dtos.FolderFileDtos;
using MainProject.API.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFilesController(IUserFileService _userFileService) : ControllerBase
    {
        /// <summary>
        /// Folder ve file-lari tek bir source-den silmek
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> BulkDeleteFoldersAndFiles([FromBody] CombinedDeleteDto dto)
        {
            if (dto.FolderPaths != null && dto.FolderPaths.Any())
            {
                await _userFileService.DeleteUsersFromFoldersAsync(dto.FolderPaths); // sql
                //await _folderService.BulkDeleteFoldersAsync(dto.FolderPaths); // mongo
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