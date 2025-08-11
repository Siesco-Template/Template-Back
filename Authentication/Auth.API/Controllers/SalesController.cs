using Auth.DAL.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController(AuthDbContext _context) : ControllerBase
    {

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllSales(int skip = 0, int take = 10)
        {
            var query = _context.Sales.AsQueryable();
            var data = await query.Select(s => new
            {
                s.Id,
                s.SaleDate,
                s.PayStatus,
                s.PayDate,
                s.CargoType,
                s.TotalAmount,
                FllName = s.User.FirstName + " " + s.User.LastName,
                s.Department.Name,
                s.Service
            })
                .Skip(Math.Max(0, skip * take))
                .Take(take)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = query.Count(),
                Data = data
            });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllDepartments(string? name)
        {
            var query = _context.Departments.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(d => d.Name.Contains(name));
            }

            var data = await query.Select(d => new
            {
                d.Id,
                d.Name,
            })
            .ToListAsync();

            return Ok(data);
        }
    }
}