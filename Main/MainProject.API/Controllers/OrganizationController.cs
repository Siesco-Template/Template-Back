using MainProject.API.Business.Dtos.OrganizationDtos;
using MainProject.API.Business.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Attributes;
using SharedLibrary.StaticDatas;
using TableComponent.Entities;
using TableComponent.Extensions;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController(GetQueryHelper _getQueryHelper, OrganizationService _organizationService) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllOrganizations([FromQuery] TableQueryRequest tableRequest)
        {
            var result = await _getQueryHelper.GetQuery(tableRequest);
            return Ok(result);
        }

        [Permission(PageKeys.Organization, ActionKeys.Create)]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationDto createOrganizationDto)
        {
            await _organizationService.CreateOrganizationAsync(createOrganizationDto);
            return Ok();
        }
    }
}