using Folder.Services.FolderFileServices;
using MainProject.API.Business.Dtos.ReportDtos;
using MainProject.API.DAL.Contexts;
using MainProject.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Enums;
using SharedLibrary.Exceptions;

namespace MainProject.API.Business.Services
{
    public class ReportService(MainDbContext _context, IFolderFileService _folderFileService)
    {
        public async Task CreateReportAsync(CreateReportDto dto)
        {
            if (!await _context.Organizations.AnyAsync(x => x.Id == dto.OrganizationId))
                throw new NotFoundException("Təşkilat mövcud deyil");

            var report = new Report
            {
                Id = Guid.NewGuid(),
                CompileDate = dto.CompileDate,
                Number = dto.Number,
                OrganizationId = dto.OrganizationId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Term = dto.Term,
                ReportStatus = dto.ReportStatus ?? ReportStatus.DaxilEdildi,
                ClassificationCode1 = dto.ClassificationCode1,
                ClassificationCode2 = dto.ClassificationCode2,
                FuntionalClassificationCode = dto.FuntionalClassificationCode,
                ReportDetails = []
            };
            await _context.Reports.AddAsync(report);

            await _context.ReportDetails.AddRangeAsync(dto.ReportDetails.Select(x => new ReportDetail
            {
                ReportId = report.Id,
                DetailId = x.DetailId,
                EstimateAmount = x.EstimateAmount,
                FinancingAmount = x.FinancingAmount,
                ActualAmount = x.ActualAmount,
                CheckoutAmount = x.CheckoutAmount,
            }));

            await _context.SaveChangesAsync();

            await _folderFileService.CreateFileAsync(

                   folderPath: "/Reports",
                   sqlId: report.Id,
                   name: $"{report.Number}"
            );
        }

        public async Task<ReportDto> GetReportByIdAsync(Guid reportId)
        {
            var report = await _context.Reports.Where(x => x.Id == reportId)
                .Select(x => new ReportDto
                {
                    Id = x.Id,
                    CompileDate = x.CompileDate,
                    EndDate = x.EndDate,
                    Number = x.Number,
                    StartDate = x.StartDate,
                    Term = x.Term,
                    ClassificationCode1 = x.ClassificationCode1,
                    ClassificationCode2 = x.ClassificationCode2,
                    OrganizationId = x.OrganizationId,
                    OrganizationName = x.Organization.Name,
                    FuntionalClassificationCode = x.FuntionalClassificationCode,
                    ReportStatus = x.ReportStatus,
                    ReportDetails = x.ReportDetails.Select(y => new ReportDetailDto
                    {
                        ActualAmount = y.ActualAmount,
                        CheckoutAmount = y.CheckoutAmount,
                        DetailCode = y.Detail.Code,
                        DetailTitle = y.Detail.Title,
                        EstimateAmount = y.EstimateAmount,
                        FinancingAmount = y.FinancingAmount
                    }).ToList()
                }).FirstOrDefaultAsync() ?? throw new NotFoundException("Report mövcud deyil!");

            return report;
        }

        public async Task DeleteReportAsync(Guid reportId)
        {
            var report = await _context.Reports.FirstOrDefaultAsync(x => x.Id == reportId) ?? throw new NotFoundException("Report mövcud deyil!");
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
            await _folderFileService.DeleteFileAsync("/Reports", report.Id.ToString());
        }

        public async Task BulkDeleteAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                throw new BadRequestException("Silinəcək istifadəçi id-ləri göndərilməyib.");

            var reports = _context.Reports.Where(u => ids.Contains(u.Id)).ToList();

            if (reports.Count == 0)
                throw new NotFoundException("Silinəcək istifadəçi tapılmadı.");

            _context.Reports.RemoveRange(reports);
            await _context.SaveChangesAsync();

            var folderPath = "/Reports";
            var fileIds = reports.Select(u => u.Id.ToString()).ToList();

            await _folderFileService.BulkDeleteFileAsync(folderPath, fileIds);
        }

        public async Task UpdateReportAsync(UpdateReportDto dto)
        {
            var report = await _context.Reports
                .Include(x => x.ReportDetails)
                .FirstOrDefaultAsync(x => x.Id == dto.Id) 
                ?? throw new NotFoundException("Report mövcud deyil!");

            if (dto.OrganizationId.HasValue && !await _context.Organizations.AnyAsync(x => x.Id == dto.OrganizationId.Value))
                throw new NotFoundException("Təşkilat mövcud deyil");

            if (dto.Number != null) report.Number = dto.Number;
            if (dto.CompileDate.HasValue) report.CompileDate = dto.CompileDate.Value;
            if (dto.StartDate.HasValue) report.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) report.EndDate = dto.EndDate.Value;
            if (dto.Term.HasValue) report.Term = dto.Term.Value;
            if (dto.ClassificationCode1 != null) report.ClassificationCode1 = dto.ClassificationCode1;
            if (dto.ClassificationCode2 != null) report.ClassificationCode2 = dto.ClassificationCode2;
            if (dto.FuntionalClassificationCode != null) report.FuntionalClassificationCode = dto.FuntionalClassificationCode;
            if (dto.OrganizationId.HasValue) report.OrganizationId = dto.OrganizationId.Value;
            if (dto.ReportStatus.HasValue) report.ReportStatus = dto.ReportStatus.Value;

            if (dto.ReportDetails != null && dto.ReportDetails.Any())
            {
                _context.ReportDetails.RemoveRange(report.ReportDetails);
                await _context.ReportDetails.AddRangeAsync(dto.ReportDetails.Select(x => new ReportDetail
                {
                    ReportId = report.Id,
                    DetailId = x.DetailId,
                    EstimateAmount = x.EstimateAmount,
                    FinancingAmount = x.FinancingAmount,
                    ActualAmount = x.ActualAmount,
                    CheckoutAmount = x.CheckoutAmount,
                }));
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateReportStatusAsync(UpdateReportStatusDto dto)
        {
            var report = await _context.Reports.FirstOrDefaultAsync(x => x.Id == dto.ReportId) 
                ?? throw new NotFoundException("Report mövcud deyil!");

            report.ReportStatus = dto.ReportStatus;
            await _context.SaveChangesAsync();
        }

        public async Task BulkUpdateReportStatusAsync(List<Guid> reportIds, ReportStatus status)
        {
            if (reportIds == null || reportIds.Count == 0)
                throw new BadRequestException("Report id-ləri göndərilməyib.");

            var reports = await _context.Reports.Where(x => reportIds.Contains(x.Id)).ToListAsync();

            if (reports.Count == 0)
                throw new NotFoundException("Report tapılmadı.");

            foreach (var report in reports)
            {
                report.ReportStatus = status;
            }

            await _context.SaveChangesAsync();
        }
    }
}