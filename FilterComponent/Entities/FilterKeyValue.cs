using FilterComponent.Enums;

namespace FilterComponent.Entities
{
    public class FilterKeyValue
    {
        /// <summary>
        /// Filterin aparıldığı sütun
        /// </summary>
        public string Column { get; set; }
        /// <summary>
        /// Filter üçün daxil edilmiş dəyər
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Filterin tipi
        /// </summary>
        public FilterOperationType FilterOperation { get; set; }
        /// <summary>
        /// Filterin hansı data tipindən dəyər üzərində aparılması
        /// </summary>
        public FilterKey FilterKey { get; set; }

        public string GetFilterQuery()
        {
            switch (FilterOperation)
            {
                case FilterOperationType.Equal:
                    return $"{Column} == \"{Value}\"";

                case FilterOperationType.NotEqual:
                    return $"{Column} != \"{Value}\"";

                case FilterOperationType.Like:
                    return $"{Column}.Contains(\"{Value}\")";

                case FilterOperationType.NotLike:
                    return $"!{Column}.Contains(\"{Value}\")";

                case FilterOperationType.GreaterThan:
                    return $"{Column} > \"{Value}\"";

                case FilterOperationType.LessThan:
                    return $"{Column} < \"{Value}\"";

                case FilterOperationType.GreaterThanOrEqual:
                    return $"{Column} >= \"{Value}\"";

                case FilterOperationType.LessThanOrEqual:
                    return $"{Column} <= \"{Value}\"";

                case FilterOperationType.In:
                    var inValues = string.Join(",", Value.Select(v => $"\"{v}\""));
                    return $"{Column} IN ({inValues})";

                case FilterOperationType.NotIn:
                    var notInValues = string.Join(",", Value.Select(v => $"\"{v}\""));
                    return $"{Column} NOT IN ({notInValues})";

                case FilterOperationType.RangeNumberOrDate:
                    string[]? parts = Value.Split(',');

                    if (parts.Length != 2)
                        throw new Exception("RangeDate filter format must be 'start,end'");

                    return $"{Column} >= \"{parts[0]}\" && {Column} <= \"{parts[1]}\"";

                //case FilterOperationType.RangeDate:
                //    var dateParts = Value.Split(',');

                //    if (dateParts.Length != 2)
                //        throw new Exception("RangeDate filter format must be 'start,end'");

                //    return $"{Column} >= \"{dateParts[0]}\" && {Column} <= \"{dateParts[1]}\"";

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
