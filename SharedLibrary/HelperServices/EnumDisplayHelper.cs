using SharedLibrary.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SharedLibrary.HelperServices
{
    public static class EnumDisplayHelper
    {
        private static readonly Dictionary<string, Type> EnumTypes = new()
        {
            { "ReportStatus", typeof(ReportStatus) },
            { "Term", typeof(Term) }
        };

        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name ?? enumValue.ToString();
        }

        public static string? GetEnumDisplayName(Type enumType, object? value)
        {
            if (value == null || !Enum.IsDefined(enumType, value))
                return value?.ToString();

            var enumValue = (Enum)Enum.ToObject(enumType, value);
            return enumValue.GetDisplayName();
        }

        public static string GetEnumDisplayNameByProperty(string propertyName, object? value)
        {
            if (EnumTypes.TryGetValue(propertyName, out var enumType))
            {
                return GetEnumDisplayName(enumType, value);
            }

            return value?.ToString() ?? string.Empty;
        }
    }
}