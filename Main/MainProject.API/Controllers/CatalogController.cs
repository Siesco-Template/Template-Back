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
        public async Task<IActionResult> GetDataByCatalogItemId(string catalogItemId, TableQueryRequest request)
        {
            var response = await _getQueryHelper.GetQuery(request);
            return Ok(response);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> FilterByCatalog([FromQuery] CatalogFilterRequest request)
        {
            var filters = new List<FilterKeyValue>
            {
                new()
                {
                    Column = request.CatalogColumn,
                    FilterOperation = FilterOperationType.Equal,
                    Value = request.CatalogValue
                }
            };

            var tableRequest = new TableQueryRequest
            {
                TableId = request.TableId,
                Columns = request.Columns,
                Filters = filters
            };

            var result = await _getQueryHelper.GetQuery(tableRequest);
            return Ok(result);
        }
    }
}