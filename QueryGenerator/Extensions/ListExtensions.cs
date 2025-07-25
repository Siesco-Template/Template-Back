namespace QueryGenerator.Extensions;

internal static class ListExtensions
{
    internal static void AddIfNotNull<T>(this IList<T> list, T? value)
    {
        if (value != null)
        {
            list.Add(value);
        }
    }
}