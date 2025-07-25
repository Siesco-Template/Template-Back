using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Attributes;
using SharedLibrary.StaticDatas;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        [Permission(PageKeys.User, ActionKeys.GetAll)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok();
        }

        [Permission(PageKeys.User, ActionKeys.Delete)]
        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteUsers()
        {
            return Ok();
        }

        [Permission(PageKeys.User, ActionKeys.Update)]
        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateUsers()
        {
            return Ok();
        }

        [Permission(PageKeys.Sale, ActionKeys.GetAll)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllSales()
        {
            return Ok();
        }

        [Permission(PageKeys.Sale, ActionKeys.Delete)]
        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteSale()
        {
            return Ok();
        }

        [Permission(PageKeys.Sale, ActionKeys.Update)]
        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateSale()
        {
            return Ok();
        }
    }
}
