using Folder.Dtos.FolderFileDtos;
using Folder.Entities;
using Folder.Roots;
using Folder.Services.FolderFileServices;
using Folder.Services.FolderServices;
using MainProject.API.Business.Dtos.FolderFiles;
using MainProject.API.Business.Dtos.OrganizationDtos;
using MainProject.API.DAL.Contexts;
using MainProject.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Exceptions;

namespace MainProject.API.Business.Services
{
    public class OrganizationFileService(MainDbContext _context, IFolderService<OrganizationFile> _folderService, IFolderFileService<OrganizationFile> _folderFileService)
    {
        public async Task CreateOrganizationAsync(CreateOrganizationFileDto dto)
        {
            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                StructuralUnit = dto.StructuralUnit,
                Code = dto.Code,
                VOEN = dto.VOEN,
                CreatedDate = dto.CreatedDate
            };
            await _context.Organizations.AddAsync(organization);

            await _context.SaveChangesAsync();
        }

        public async Task<FileDto> CreateAsync(CreateOrganizationFileDto dto)
        {
            string nextCode = await _folderFileService.GenerateNextFileCodeAsync(RootFolders.Organizations, "org", 3);

            await _folderService.InitializeRootFolder();

            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                StructuralUnit = dto.StructuralUnit,
                Code = dto.Code,
                VOEN = dto.VOEN,
                CreatedDate = dto.CreatedDate
            };
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();

            var organizationFile = new OrganizationFile
            {
                Id = organization.Id,
                Name = organization.Name,
                FileName = organization.Name,
                Code = nextCode,
                CreateDate = organization.CreatedDate
            };

            await _folderFileService.AddFileToFolderAsync(dto.FolderPath, organizationFile);

            return new FileDto
            {
                Id = organizationFile.Id,
                FileName = organizationFile.FileName,
                CreateDate = organizationFile.CreateDate
            };
        }

        public async Task SyncAllOrganizationsToFolderAsync()
        {
            var organizations = await _context.Organizations.ToListAsync();

            await _folderService.InitializeRootFolder();

            var mainFolder = await _folderService.GetFolderByPathAsync(RootFolders.Organizations);
            if (mainFolder == null)
                throw new BadRequestException("Əsas qovluq tapılmadı.");

            // 1. Əvvəlcə hazırda var olan max kodu tap
            var existingCodes = mainFolder.Files
                .OfType<BaseFile>()
                .Where(f => f.Code.StartsWith("org"))
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

            var organizationFiles = new List<OrganizationFile>();
            foreach (var org in organizations)
            {
                maxNumber++;
                string code = $"org_{maxNumber.ToString().PadLeft(3, '0')}";

                organizationFiles.Add(new OrganizationFile
                {
                    Id = org.Id,
                    Name = org.Name,
                    VOEN = org.VOEN,
                    FileName = $"{org.Name}",
                    Code = code,
                    CreateDate = org.CreatedDate
                });
            }

            await _folderFileService.AddFilesToFolderAsync(RootFolders.Organizations, organizationFiles);
        }
    }
}