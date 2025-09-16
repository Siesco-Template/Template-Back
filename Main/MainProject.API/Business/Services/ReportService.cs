using MainProject.API.Business.Dtos.ReportDtos;
using MainProject.API.DAL.Contexts;
using MainProject.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Enums;
using SharedLibrary.Exceptions;

namespace MainProject.API.Business.Services
{
    public class ReportService(MainDbContext _context)
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
                ReportStatus = ReportStatus.Compiled,
                ClassificationCode1 = dto.ClassificationCode1,
                ClassificationCode2 = dto.ClassificationCode2,
                FuntionalClassificationCode = dto.FuntionalClassificationCode
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
    }
}