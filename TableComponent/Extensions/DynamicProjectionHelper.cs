using QueryGenerator.Core;

namespace TableComponent.Extensions
{
    public static class DynamicProjectionHelper
    {
        public static IQueryable<dynamic> GetSelectedColumns(this IQueryable<dynamic> query, string columns)
        {
            if (string.IsNullOrWhiteSpace(columns))
                throw new ArgumentException("Columns string cannot be null or empty.", nameof(columns));

            return (IQueryable<dynamic>)query.Select($"new({columns})");
        }
    }
}