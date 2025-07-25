using MassTransit;
using Permission.Api.Entities;
using Permission.Api.Services;
using SharedLibrary.Events;

namespace Permission.Api.Consumers
{
    public class UserRegisteredPermissionConsumer(IPermissionService _service) : IConsumer<UserRegisteredPermissionEvent>
    {
        public async Task Consume(ConsumeContext<UserRegisteredPermissionEvent> context)
        {
            var userEvent = context.Message;

            var newUserPermission = new UserPermission
            {
                UserId = userEvent.UserId,
                FullName = userEvent.FullName,
                IsBlocked = userEvent.IsBlocked,
                Permissions = new List<UserPagePermission>()
            };

            await _service.AddUserPermissionAsync(newUserPermission);
        }
    }
}
