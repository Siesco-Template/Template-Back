using Permission.Api.Dtos;
using Permission.Api.Entities;
using SharedLibrary.Dtos.Common;
using SharedLibrary.Dtos.PermissionDtos;
using SharedLibrary.Requests;

namespace Permission.Api.Services
{
    public interface IPermissionService
    {
        Task<List<PageDto>> GetAllPagesAndActionsAsync();
        Task<bool> UpdateUserPermissionsAsync(List<UpdateUserPermissionsDto> updatedPermissions);
        Task<DataListDto<UserPermissionsDto>> GetAllUserPermissions(int skip = 0, int take = 10);
        Task<UserPermissionsDto> GetUserPermissionsById(Guid userId);
        Task<UserPermissionsDto?> GetCurrentUserPermissionsAsync();
        Task<bool> CheckPermissionAsync(Guid userId, string page, string action);
        Task<bool> AddUserPermissionAsync(UserPermission userPermission);
        Task<List<string>> SyncPagesAndActionsAsync(PermissionRegisteredRequest permissionData);
        Task SyncUsersAsync();
    }
}