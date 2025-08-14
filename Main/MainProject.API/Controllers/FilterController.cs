using FilterComponent.Dtos;
using FilterComponent.Services;
using Microsoft.AspNetCore.Mvc;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterController(FilterService _filterService) : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateFilter(CreateFilterDto dto)
        {
            await _filterService.SaveFilter(dto);
            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetFiltersByTableId(string tableId)
        {
            return Ok(await _filterService.GetAllFilters(tableId));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetFilterById(string filterId)
        {
            return Ok(await _filterService.GetFilterById(filterId));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetDefaultFilter(string tableId)
        {
            return Ok(await _filterService.GetDefaultFilter(tableId));
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> SetDefaultFilter(string filterId)
        {
            await _filterService.SetDefault(filterId);
            return Ok();
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> RemoveDefaultFilter(string filterId)
        {
            await _filterService.RemoveDefault(filterId);
            return Ok();
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateFilter(string filterId, UpdateFilterDto dto)
        {
            await _filterService.UpdateFilter(filterId, dto);
            return Ok();
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteFilter(string filterId)
        {
            await _filterService.DeleteFilter(filterId);
            return Ok();
        }
    }
}