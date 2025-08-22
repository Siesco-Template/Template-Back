using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Permission.Api.Dtos;
using Permission.Api.Services;
using SharedLibrary.Attributes;
using SharedLibrary.Dtos.PermissionDtos;
using SharedLibrary.HelperServices.Permission;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using SharedLibrary.StaticDatas;
using System.Reflection;
using System.Text.Json;

namespace Permission.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController(IPermissionService _service,
                                       IConfiguration _configuration,
                                       PageAndActionScannerForServices _scanner,
                                       IRequestClient<PermissionRegisteredRequest> _requestClient) : ControllerBase
    {

        [HttpPost("[action]")]
        public async Task<IActionResult> SyncUsers()
        {
            await _service.SyncUsersAsync();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SyncAllMicroservicePagesAndActions()
        {
            // Burda icinde permission attributu olan servisler yazilir
            // ve o servislerde /PermissionScanner/SyncPagesAndActions endpoint-i yazilmalidir(endpoint daxilinde sorgu geden metod shared-dedir)
            var urls = new List<string>
                       {
                           $"{_configuration["AuthService:BaseUrl"]}/PermissionScanner/SyncPagesAndActions", 
                           //"http://localhost:5002/api/PermissionScanner/SyncPagesAndActions",
                       };

            using var httpClient = new HttpClient();

            var allPages = new List<PageDto>();

            foreach (var url in urls)
            {
                var response = await httpClient.PostAsync(url, null);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var remotePages = JsonSerializer.Deserialize<List<PageDto>>(content, options);

                    if (remotePages != null)
                        allPages.AddRange(remotePages);
                }
            }

            // permission servisinin oz attributlarını da əlavə et
            var localControllerTypes = Assembly.GetEntryAssembly().GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

            var localPages = await _scanner.ScanPagesOnlyAsync(localControllerTypes);
            allPages.AddRange(localPages);

            // toplanmış bütün page və actionları bir yerdə DB-ə göndər
            var request = new PermissionRegisteredRequest { Pages = allPages };
            var responseFromConsumer = await _requestClient.GetResponse<PermissionRegisteredResponse>(request);

            return Ok(responseFromConsumer.Message.LogMessages);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllPagesAndActions()
        {
            return Ok(await _service.GetAllPagesAndActionsAsync());
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateUserPermissions([FromBody] List<UpdateUserPermissionsDto> updatedPermissions)
        {
            return Ok(await _service.UpdateUserPermissionsAsync(updatedPermissions));
        }

        [Permission(PageKeys.Permission, ActionKeys.GetAll)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUserPermissions(int skip = 0, int take = 10)
        {
            return Ok(await _service.GetAllUserPermissions(skip, take));
        }

        [Permission(PageKeys.Permission, ActionKeys.GetAll)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetUserPermissionsById(Guid userId)
        {
            return Ok(await _service.GetUserPermissionsById(userId));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetCurrentUserPermissions()
        {
            return Ok(await _service.GetCurrentUserPermissionsAsync());
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CheckPermission(Guid userId, string page, string action)
        {
            var hasPermission = await _service.CheckPermissionAsync(userId, page, action);
            return Ok(new { hasPermission });
        }

    }
}
