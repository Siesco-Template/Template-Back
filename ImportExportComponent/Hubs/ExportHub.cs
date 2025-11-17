using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;

namespace ImportExportComponent.Hubs
{
    // Authorization is handled at the endpoint level, not at hub level for CORS compatibility
    public class ExportHub(ILogger<ExportHub> logger) : Hub
    {
        private readonly ILogger<ExportHub> _logger = logger;

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Yeni bağlantı: {ConnectionId}", Context.ConnectionId);

            if (!string.IsNullOrWhiteSpace(Context.UserIdentifier))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception is not null)
            {
                _logger.LogWarning(exception, "Bağlantı kəsildi: {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Bağlantı kəsildi: {ConnectionId}", Context.ConnectionId);
            }

            if (!string.IsNullOrWhiteSpace(Context.UserIdentifier))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.UserIdentifier);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public Task JoinUserGroup(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("İstifadəçi ID boş ola bilməz.", nameof(userId));
            }

            return Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}
