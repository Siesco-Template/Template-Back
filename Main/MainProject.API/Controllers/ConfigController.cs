using ConfigComponent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController(ConfigService configService) : ControllerBase
    {
        private readonly ConfigService _configService = configService;

        [HttpGet("[action]")]
        public async Task<IActionResult> GetDefaultAndUserConfig()
        {
            var config = await _configService.GetDefaultAndUserConfigAsync();
            return Ok(config);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateOrUpdate(Dictionary<string, object> overrides)
        {
            await _configService.CreateOrUpdateUserConfigAsync(overrides);
            return Ok();
        }

        [Authorize]
        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete(string tableKey)
        {
            await _configService.DeleteUserTableConfigAsync(tableKey);
            return Ok();
        }
    }
}