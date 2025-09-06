using MainProject.API.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController(CatalogService catalogService) : ControllerBase
    {
        private readonly CatalogService _catalogService = catalogService;

        [HttpGet("[action]")]
        public async Task<IActionResult> GetCatalogs(string tableId)
        {
            var response = await _catalogService.GetCatalogs(tableId);
            return Ok(response);
        }
    }
}