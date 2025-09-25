using Microsoft.AspNetCore.Mvc;
using TableComponent.Entities;
using TableComponent.Extensions;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController(GetQueryHelper _getQueryHelper) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllOrganizations([FromQuery] TableQueryRequest tableRequest)
        {
            var result = await _getQueryHelper.GetQuery(tableRequest);
            return Ok(result);
        }
    }
}