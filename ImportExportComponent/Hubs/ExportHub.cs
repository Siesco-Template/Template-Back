using Microsoft.AspNetCore.SignalR;

namespace ImportExportComponent.Hubs
{
    public class ExportHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Yeni bağlantı: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Bağlantı kəsildi: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}