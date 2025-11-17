using MainProject.API.Business.Dtos.ReportDtos;
using MainProject.API.Business.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Attributes;
using SharedLibrary.StaticDatas;
using TableComponent.Entities;
using TableComponent.Extensions;

namespace MainProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController(ReportService _reportService, GetQueryHelper _getQueryHelper) : ControllerBase
    {
        [Permission(PageKeys.Report, ActionKeys.Create)]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto createReportDto)
        {
            await _reportService.CreateReportAsync(createReportDto);
            return Ok();
        }

        [Permission(PageKeys.Report, ActionKeys.GetById)]
        [HttpGet("[action]")]
        public async Task<ReportDto> GetReportById(Guid reportId) => await _reportService.GetReportByIdAsync(reportId);

        //[Permission(PageKeys.Report, ActionKeys.GetById)]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllReports([FromQuery] TableQueryRequest tableRequest)
        {
            var result = await _getQueryHelper.GetQuery(tableRequest);
            return Ok(result);
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteReport(Guid reportId)
        {
            await _reportService.DeleteReportAsync(reportId);
            return Ok();
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> BulkDeleteReport(List<Guid> reportIds)
        {
            await _reportService.BulkDeleteAsync(reportIds);
            return Ok();
        }

        [Permission(PageKeys.Report, ActionKeys.Update)]
        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateReport([FromBody] UpdateReportDto updateReportDto)
        {
            await _reportService.UpdateReportAsync(updateReportDto);
            return Ok();
        }

        [Permission(PageKeys.Report, ActionKeys.Update)]
        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateReportStatus([FromBody] UpdateReportStatusDto updateReportStatusDto)
        {
            await _reportService.UpdateReportStatusAsync(updateReportStatusDto);
            return Ok();
        }

        [Permission(PageKeys.Report, ActionKeys.Update)]
        [HttpPut("[action]")]
        public async Task<IActionResult> BulkUpdateReportStatus([FromBody] BulkUpdateReportStatusDto bulkUpdateDto)
        {
            await _reportService.BulkUpdateReportStatusAsync(bulkUpdateDto.ReportIds, bulkUpdateDto.ReportStatus);
            return Ok();
        }
    }

    public class BulkUpdateReportStatusDto
    {
        public List<Guid> ReportIds { get; set; }
        public SharedLibrary.Enums.ReportStatus ReportStatus { get; set; }
    }
}