using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Attributes;
using SharedLibrary.StaticDatas;
using TableComponent.Entities;
using TableComponent.Extensions;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(GetQueryHelper _getQueryHelper) : ControllerBase
    {
        [Permission(PageKeys.User, ActionKeys.GetAll)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUsers([FromQuery] TableQueryRequest tableRequest)
        {
            var result = await _getQueryHelper.GetQuery(tableRequest);
            return Ok(result);
        }
    }
}