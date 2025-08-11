using Auth.Business.Dtos;
using Auth.DAL.Contexts;
using FilterComponent.Dtos;
using FilterComponent.Services;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Dtos.Common;
using SharedLibrary.Enums;

namespace Auth.Business.Services
{
    public class GetUserService(AuthDbContext _context, FilterService _filterService)
    {
        public async Task<DataListDto<UserListDto>> GetAllUsersAsync(FilterDto filter, int skip = 0, int take = 25)
        {
            var query = _context.Users
                .Where(x => x.UserRole != UserRole.SuperAdmin)
                .OrderByDescending(x => x.RegistrationDate)
                .AsNoTracking()
                .AsQueryable();

            query = await _filterService.ApplyFilter(query, filter);

            var datas = await query
            .Select(x => new UserListDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                AppUserRole = x.UserRole,
                IsBlock = x.IsBlock
            })
            .Skip(skip * take)
            .Take(take)
            .ToListAsync();

            return new DataListDto<UserListDto>
            {
                Datas = datas,
                TotalCount = await query.CountAsync()
            };
        }
    }
}