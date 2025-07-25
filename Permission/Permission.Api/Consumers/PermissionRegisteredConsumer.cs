using MassTransit;
using Permission.Api.Services;
using SharedLibrary.Requests;
using SharedLibrary.Responses;

namespace Permission.Api.Consumers
{
    public class PermissionRegisteredConsumer(IPermissionService _service) : IConsumer<PermissionRegisteredRequest>
    {
        public async Task Consume(ConsumeContext<PermissionRegisteredRequest> context)
        {
            var logMessages = await _service.SyncPagesAndActionsAsync(context.Message);

            await context.RespondAsync(new PermissionRegisteredResponse
            {
                LogMessages = logMessages
            }) ;
        }
    }
}
