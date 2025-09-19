using MainProject.API.Business.Dtos.OrganizationDtos;
using MainProject.API.Business.Dtos.ReportFileDtos;
using MainProject.API.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationFileController(OrganizationFileService _organizationFileService) : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> SyncOrganizationsToFolder()
        {
            await _organizationFileService.SyncAllOrganizationsToFolderAsync();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationFileDto dto)

        {
            return Ok(await _organizationFileService.CreateAsync(dto));
        }
    }
}