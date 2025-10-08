using FilterComponent.Entities;
using FilterComponent.Enums;
using Microsoft.AspNetCore.Mvc;
using TableComponent.Entities;
using TableComponent.Extensions;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController(GetQueryHelper _getQueryHelper) : ControllerBase
    {
        [HttpGet("[action]")]
        public async Task<IActionResult> GetCatalog([FromQuery] CatalogQueryRequest catalogRequest)
        {
            var result = await _getQueryHelper.GetCatalog(catalogRequest);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> FilterByCatalog([FromQuery] CatalogFilterRequest request)
        {
            var result = await _getQueryHelper.GetQuery(request);
            return Ok(result);
        }
    }
}