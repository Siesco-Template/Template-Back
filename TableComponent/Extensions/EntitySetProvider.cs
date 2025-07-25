using Microsoft.EntityFrameworkCore;

namespace TableComponent.Extensions
{
    public class EntitySetProvider(DbContext context)
    {
        private readonly DbContext _context = context;
        public IQueryable GetQueryable(Type entityType)
        {
            var set = _context.GetType()
                .GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
                .MakeGenericMethod(entityType)
                .Invoke(_context, null)!;

            return (IQueryable)set;
        }

        public Type? GetEntityType(string tableName)
        {
            return _context.Model.GetEntityTypes()
                .FirstOrDefault(et => string.Equals(et.GetTableName(), tableName, StringComparison.OrdinalIgnoreCase))
                ?.ClrType ?? throw new Exception("Cədvəl tapılmadı.");
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        public async Task AddRangeAsync(IEnumerable<object> entities)
        {
            await _context.AddRangeAsync(entities);
        }
    }
}