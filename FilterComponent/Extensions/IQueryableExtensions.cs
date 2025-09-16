using FilterComponent.Entities;

namespace FilterComponent.Extensions
{
    public class IQueryableExtensions
    {
        /// <summary>
        /// Daxil edilmiş FilterKeyValue-lara əsasən sorğunun yaradılması
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static string? GenerateQuery(List<FilterKeyValue> filters)
        {
            if (filters != null)
            {
                return string.Join(" && ", filters.Select(filter => filter.GetFilterQuery()));
            }
            else
                return null;
        }

        /// <summary>
        /// Catalog üçün sadə LIKE axtarışı (Column1.Contains(value) || Column2.Contains(value))
        /// </summary>
        public static string? GenerateCatalogQuery(string? columns, string? searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchValue) || string.IsNullOrWhiteSpace(columns))
                return null;

            var columnList = columns
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c));

            var conditions = columnList
                .Select(c => $"{c}.Contains(\"{searchValue}\")");

            return string.Join(" || ", conditions);
        }
    }
}