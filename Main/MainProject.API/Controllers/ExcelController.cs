using ImportExportComponent.Dtos;
using ImportExportComponent.HelperServices;
using ImportExportComponent.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.HelperServices;
using System.Security.Claims;
using System.Threading.Channels;
using TableComponent.Entities;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController(ImportService importService, Channel<(List<ExportColumnDto>, TableQueryRequest, string)> queue, DownloadItemService downloadItemService, RuleProvider ruleProvider) : ControllerBase
    {
        private readonly ImportService _service = importService;
        private readonly RuleProvider _ruleProvider = ruleProvider;
        private readonly Channel<(List<ExportColumnDto> columns, TableQueryRequest tableRequest, string userId)> _queue = queue;
        private readonly DownloadItemService _downloadItemService = downloadItemService;

        //[Permission(PageKeys.Report, ActionKeys.UploadFile)]
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var data = await _service.ExtractExcelPreviewAsync(file);
            return Ok(data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Validate([FromBody] ValidateRequestDto request)
        {
            var data = await _service.ValidateRecords(request);
            return Ok(data);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetValidationRules(string tableName)
        {
            var data = await _ruleProvider.GetRulesAsync(tableName);
            return Ok(data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Save(ImportConfirmRequestDto dto)
        {
            var data = await _service.SaveRecords(dto);
            return Ok(data);
        }

        //[Permission(PageKeys.Report, ActionKeys.ExportFile)]
        [HttpPost("[action]")]
        public async Task<IActionResult> StartExport([FromBody] ExportRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _queue.Writer.WriteAsync((request.Columns, request.TableRequest, userId));
            return Ok(new { message = "Export başlatıldı. Zəhmət olmasa tamamlanmasını gözləyin." });
        }

        //[Permission(PageKeys.Report, ActionKeys.GetAll)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetItems()
        {
            var data = await _downloadItemService.GetUserDownloads();
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