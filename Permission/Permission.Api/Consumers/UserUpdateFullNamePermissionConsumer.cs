using MassTransit;
using Permission.Api.Services;
using SharedLibrary.Events;

namespace Permission.Api.Consumers
{
    public class UserUpdateFullNamePermissionConsumer(IPermissionService _permissionService) : IConsumer<UserUpdateFullNamePermissionEvent>
    {
        public async Task Consume(ConsumeContext<UserUpdateFullNamePermissionEvent> context)
        {
            var userEvent = context.Message;

            //await _permissionService.UpdateUserFullNameAsync(userEvent.UserId, userEvent.FullName);
        }
    }
}