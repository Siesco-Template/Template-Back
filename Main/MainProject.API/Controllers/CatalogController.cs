using MainProject.API.Business.Services;
using Microsoft.AspNetCore.Mvc;
using TableComponent.Entities;
using TableComponent.Extensions;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController(CatalogService catalogService, GetQueryHelper _getQueryHelper) : ControllerBase
    {
        private readonly CatalogService _catalogService = catalogService;

        [HttpGet("[action]")]
        public async Task<IActionResult> GetCatalogs(string tableId)
        {
            var response = await _catalogService.GetCatalogs(tableId);
            return Ok(response);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetCatalog([FromQuery] CatalogQueryRequest catalogRequest)
        {
            var result = await _getQueryHelper.GetCatalog(catalogRequest);
            return Ok(result);
        }
    }
}