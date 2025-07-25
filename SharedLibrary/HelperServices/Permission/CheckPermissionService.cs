using SharedLibrary.Dtos.PermissionDtos;
using System.Net.Http.Json;

namespace SharedLibrary.HelperServices.Permission
{
    public class CheckPermissionService : ICheckPermissionService
    {
        private readonly HttpClient _httpClient;
        private readonly PermissionServiceConfig _config;

        public CheckPermissionService(HttpClient httpClient, PermissionServiceConfig config)
        {
            _httpClient = httpClient;
            _config = config;
        }
        /// <summary>
        /// user-in verilen page ve action ucun icazesinin olub olmamasini yoxlayir
        /// </summary>
        public async Task<bool> CheckPermission(Guid userId, string page, string action)
        {
            var url = $"{_config.BaseUrl}/Permissions/CheckPermission?userId={userId}&page={page}&action={action}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PermissionCheckResponse>();
                return result?.HasPermission ?? false;
            }

            return false;
        }
    }

    public interface ICheckPermissionService
    {
        Task<bool> CheckPermission(Guid userId, string page, string action);
    }

}
