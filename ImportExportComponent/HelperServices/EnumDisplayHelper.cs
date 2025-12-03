using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ImportExportComponent.HelperServices
{
    public static class EnumDisplayHelper
    {
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
    }
}

