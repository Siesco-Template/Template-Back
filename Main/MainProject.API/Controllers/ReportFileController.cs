using MainProject.API.Business.Dtos.ReportFileDtos;
using MainProject.API.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportFileController(ReportFileService _reportFileService) : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> SyncReportsToFolder()
        {
            await _reportFileService.SyncAllReportsToFolderAsync();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportFileDto dto)

        {
            return Ok(await _reportFileService.CreateAsync(dto));
        }
    }
}