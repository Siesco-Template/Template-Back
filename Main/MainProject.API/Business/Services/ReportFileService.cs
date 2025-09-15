using Folder.Dtos.FolderFileDtos;
using Folder.Entities;
using Folder.Roots;
using Folder.Services.FolderFileServices;
using Folder.Services.FolderServices;
using MainProject.API.Business.Dtos.FolderFiles;
using MainProject.API.Business.Dtos.ReportFileDtos;
using MainProject.API.DAL.Contexts;
using MainProject.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Enums;
using SharedLibrary.Exceptions;

namespace MainProject.API.Business.Services
{
    public class ReportFileService(MainDbContext _context, IFolderService<ReportFile> _folderService, IFolderFileService<ReportFile> _folderFileService)
    {
        /// <summary>
        /// folder mentiqi sonradan qosulursa var olan file-lari cekmek ucun
        /// </summary>
        public async Task SyncAllReportsToFolderAsync()
        {
            var reports = await _context.Reports.ToListAsync();

            await _folderService.InitializeRootFolder();

            var mainFolder = await _folderService.GetFolderByPathAsync(RootFolders.Reports);
            if (mainFolder == null)
                throw new BadRequestException("Əsas qovluq tapılmadı.");

            // 1. Əvvəlcə hazırda var olan max kodu tap
            var existingCodes = mainFolder.Files
                .OfType<BaseFile>()
                .Where(f => f.Code.StartsWith("rpt"))
                .Select(f => f.Code)
                .ToList();

            int maxNumber = 0;
            foreach (var code in existingCodes)
            {
                var parts = code.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[1], out int number))
                {
                    if (number > maxNumber)
                        maxNumber = number;
                }
            }

            var reportFiles = new List<ReportFile>();
            foreach (var report in reports)
            {
                maxNumber++;
                string code = $"rpt_{maxNumber.ToString().PadLeft(3, '0')}";

                reportFiles.Add(new ReportFile
                {
                    Id = report.Id,
                    Number = report.Number,
                    OrganizationName = report.ClassificationCode1,
                    FileName = $"{report.Number}",
                    Code = code,
                    CreateDate = report.CompileDate
                });
            }

            await _folderFileService.AddFilesToFolderAsync(RootFolders.Reports, reportFiles);
        }

        public async Task<FileDto> CreateAsync(CreateReportFileDto dto)
        {
            string nextCode = await _folderFileService.GenerateNextFileCodeAsync(RootFolders.Reports, "rpt", 3);

            await _folderService.InitializeRootFolder();

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

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            var reportFile = new ReportFile
            {
                Id = report.Id,
                Number = report.Number,
                FileName = report.Number,
                Code = nextCode,
                CreateDate = report.CompileDate,
            };

            await _folderFileService.AddFileToFolderAsync(dto.FolderPath, reportFile);

            return new FileDto
            {
                Id = reportFile.Id,
                FileName = reportFile.FileName,
                CreateDate = reportFile.CreateDate
            };
        }
    }
}