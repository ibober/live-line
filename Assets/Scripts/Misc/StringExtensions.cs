using System.Collections;

internal static class StringExtensions
{
    public static string DecideEnding(this ICollection collection)
    {
        return collection.Count == 1 ? string.Empty : "s";
    }
}
