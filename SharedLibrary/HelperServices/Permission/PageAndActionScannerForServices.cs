using MassTransit;
using SharedLibrary.Dtos.PermissionDtos;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using System.Reflection;

namespace SharedLibrary.HelperServices.Permission
{
    public class PageAndActionScannerForServices(IRequestClient<PermissionRegisteredRequest> _requestClient)
    {
        /// <summary>
        /// microservice strukturunda islemesi ucun diger servislerden permission attributunun oxunmasi 
        /// </summary>
        public async Task<List<string>> ScanAndSendPagesAndActionsAsync(IEnumerable<Type>? controllerTypes)
        {
            var permissions = new List<PageDto>();

            foreach (var controller in controllerTypes)
            {
                var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    var permissionAttributes = method.GetCustomAttributes<Attributes.PermissionAttribute>(true);
                    foreach (var attr in permissionAttributes)
                    {
                        permissions.Add(new PageDto
                        {
                            Key = attr.PageKey,
                            Name = attr.PageName,
                            Actions = new List<ActionDto>
                                {
                                    new ActionDto { Key = attr.ActionKey, Name = attr.ActionName }
                                }
                        });
                    }
                }
            }

            if (!permissions.Any())
                return new();

            var request = new PermissionRegisteredRequest
            {
                Pages = permissions
            };

            var response = await _requestClient.GetResponse<PermissionRegisteredResponse>(request);
            return response.Message.LogMessages;
        }

        public async Task<List<PageDto>> ScanPagesOnlyAsync(IEnumerable<Type>? controllerTypes)
        {
            var permissions = new List<PageDto>();

            foreach (var controller in controllerTypes)
            {
                var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    var permissionAttributes = method.GetCustomAttributes<Attributes.PermissionAttribute>(true);
                    foreach (var attr in permissionAttributes)
                    {
                        permissions.Add(new PageDto
                        {
                            Key = attr.PageKey,
                            Name = attr.PageName,
                            Actions = new List<ActionDto>
                        {
                            new ActionDto { Key = attr.ActionKey, Name = attr.ActionName }
                        }
                        });
                    }
                }
            }

            return permissions;
        }
    }
}
