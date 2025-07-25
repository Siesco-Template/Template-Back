using Microsoft.AspNetCore.Mvc;
using SharedLibrary.HelperServices.Permission;
using System.Reflection;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionScannerController(PageAndActionScannerForServices _scanner) : ControllerBase
    {

        [HttpPost("[action]")]
        public async Task<IActionResult> SyncPagesAndActions()
        {
            var controllerTypes = Assembly.GetEntryAssembly().GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

            var pages = await _scanner.ScanPagesOnlyAsync(controllerTypes);
            return Ok(pages); // DİQQƏT: artıq log yox, sadəcə PageDto siyahısı qaytarılır
        }

        //[HttpPost("[action]")]
        //public async Task<IActionResult> SyncPagesAndActions()
        //{
        //    var controllerTypes = Assembly.GetEntryAssembly().GetTypes()
        //        .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

        //    var logMessages = await _scanner.ScanAndSendPagesAndActionsAsync(controllerTypes);
        //    return Ok(logMessages);
        //}
    }
}