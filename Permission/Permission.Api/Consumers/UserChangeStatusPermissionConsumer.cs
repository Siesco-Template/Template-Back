using MassTransit;
using Permission.Api.Services;
using SharedLibrary.Events;

namespace Permission.Api.Consumers
{
    public class UserChangeStatusPermissionConsumer(IPermissionService _service) : IConsumer<UserChangeStatusPermissionEvent>
    {
        public async Task Consume(ConsumeContext<UserChangeStatusPermissionEvent> context)
        {
            var userEvent = context.Message;

            //await _service.ChangeUserStatusAsync(userEvent.UserId, userEvent.IsActive);
        }
    }
}