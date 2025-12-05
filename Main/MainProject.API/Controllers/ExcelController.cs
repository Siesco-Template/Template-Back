using ImportExportComponent.BackgroundServices;
using ImportExportComponent.Dtos;
using ImportExportComponent.HelperServices;
using ImportExportComponent.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Channels;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController(
        ImportService importService,
        Channel<ExportQueueItem> queue,
        DownloadItemService downloadItemService,
        RuleProvider ruleProvider) : ControllerBase
    {
        private readonly ImportService _service = importService;
        private readonly RuleProvider _ruleProvider = ruleProvider;
        private readonly Channel<ExportQueueItem> _queue = queue;
        private readonly DownloadItemService _downloadItemService = downloadItemService;

        //[Permission(PageKeys.Report, ActionKeys.UploadFile)]
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var data = await _service.ExtractExcelPreviewAsync(file, HttpContext.RequestAborted);
            return Ok(data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Validate([FromBody] ValidateRequestDto request)
        {
            var data = await _service.ValidateRecords(request, HttpContext.RequestAborted);
            return Ok(data);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetValidationRules(string tableName)
        {
            var data = await _ruleProvider.GetRulesAsync(tableName, HttpContext.RequestAborted);
            return Ok(data);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetTableColumns(string tableName)
        {
            var data = await _service.GetTableColumnsAsync(tableName, HttpContext.RequestAborted);
            return Ok(data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Save(ImportConfirmRequestDto dto)
        {
            var data = await _service.SaveRecords(dto, HttpContext.RequestAborted);
            return Ok(data);
        }

        //[Permission(PageKeys.Report, ActionKeys.ExportFile)]
        [HttpPost("[action]")]
        public async Task<IActionResult> StartExport([FromBody] ExportRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("İstifadəçi məlumatları tapılmadı.");
            }

            var item = new ExportQueueItem(request.Columns, request.TableRequest, userId);
            await _queue.Writer.WriteAsync(item, HttpContext.RequestAborted);
            return Ok(new { message = "Export başlatıldı. Zəhmət olmasa tamamlanmasını gözləyin." });
        }

        //[Permission(PageKeys.Report, ActionKeys.GetAll)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetItems()
        {
            var data = await _downloadItemService.GetUserDownloads(HttpContext.RequestAborted);
            return Ok(data);
        }

        //[HttpPost("[action]")]
        //public async Task<IActionResult> ExtractBudceSheet(IFormFile file)
        //{
        //    var data = await _spesificExcel.ExtractBudceSheetAsync(file);
        //    return Ok(data);
        //}
    }
}