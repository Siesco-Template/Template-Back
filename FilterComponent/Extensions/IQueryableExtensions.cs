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
    }
}