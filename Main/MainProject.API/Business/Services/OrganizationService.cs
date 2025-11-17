using Folder.Services.FolderFileServices;
using MainProject.API.Business.Dtos.OrganizationDtos;
using MainProject.API.DAL.Contexts;
using MainProject.API.Domain.Entities;

namespace MainProject.API.Business.Services
{
    public class OrganizationService(MainDbContext _context, IFolderFileService _folderFileService)
    {
        public async Task CreateOrganizationAsync(CreateOrganizationDto dto)
        {
            var organization = new Organization
            {
                Name = dto.Name,
                StructuralUnit = dto.StructuralUnit,
                VOEN = dto.VOEN,
                Code = dto.Code
            };

            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();

            await _folderFileService.CreateFileAsync(

                   folderPath: "/Organizations",
                   sqlId: organization.Id,
                   name: $"{organization.Name}_{organization.Code}"
            );
        }
    }
}