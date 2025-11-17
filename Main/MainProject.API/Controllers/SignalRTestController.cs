using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ImportExportComponent.Hubs;

namespace MainProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignalRTestController : ControllerBase
    {
        private readonly IHubContext<ExportHub> _hubContext;
        private readonly ILogger<SignalRTestController> _logger;

        public SignalRTestController(IHubContext<ExportHub> hubContext, ILogger<SignalRTestController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// SignalR bağlantısını test etmek için kullanılır
        /// </summary>
        [HttpPost("test-connection")]
        [Authorize]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value ?? User.Identity?.Name ?? "unknown";
                
                _logger.LogInformation("Test connection request from user: {UserId}", userId);

                // Kullanıcıya test mesajı gönder
                await _hubContext.Clients.User(userId).SendAsync("testMessage", new
                {
                    Message = "SignalR bağlantısı başarılı!",
                    Timestamp = DateTime.UtcNow,
                    UserId = userId
                });

                return Ok(new
                {
                    Success = true,
                    Message = "Test mesajı gönderildi. Frontend'de 'testMessage' event'ini dinleyin.",
                    UserId = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test connection failed");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// SignalR hub durumunu kontrol eder
        /// </summary>
        [HttpGet("hub-status")]
        public IActionResult GetHubStatus()
        {
            return Ok(new
            {
                HubName = nameof(ExportHub),
                HubPath = "/hubs/export",
                Status = "Active",
                Endpoints = new[]
                {
                    "/hubs/export/negotiate",
                    "/hubs/export"
                }
            });
        }
    }
}







