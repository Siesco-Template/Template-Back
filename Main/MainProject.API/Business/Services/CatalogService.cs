using MainProject.API.Business.Dtos.CatalogDto;
using MainProject.API.DAL.Contexts;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;

namespace MainProject.API.Business.Services
{
    public class CatalogService(MainDbContext context)
    {
        private readonly MainDbContext _context = context;
        public async Task<List<CatalogListDto>> GetCatalogs(string tableId)
        {
            var catalog = await _context.TableCatalogs
                .Where(tc => tc.TableId == tableId)
                .Select(tc => new CatalogListDto { CatalogId = tc.CatalogId , CatalogPath = tc.CatalogPath}).ToListAsync();
            return catalog;
        }
    }
}