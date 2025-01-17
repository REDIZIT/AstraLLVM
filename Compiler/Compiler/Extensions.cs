public static class Extensions
{
    public static int IndexOf<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
        int i = 0;
        foreach (T item in list)
        {
            if (predicate(item)) return i;
            i++;
        }

        return -1;
    }
}