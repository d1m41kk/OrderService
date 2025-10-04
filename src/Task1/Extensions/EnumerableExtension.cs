namespace Task1.Extensions;

public static class EnumerableExtension
{
    public static IEnumerable<T[]> ZipCollections<T>(
        this IEnumerable<T> firstCollection,
        params IEnumerable<T>[] otherCollections)
    {
        ArgumentNullException.ThrowIfNull(firstCollection);
        ArgumentNullException.ThrowIfNull(otherCollections);

        var enumerators = new List<IEnumerator<T>>(1 + otherCollections.Length);

        try
        {
            IEnumerator<T> enumerator = firstCollection.GetEnumerator();
            enumerators.Add(enumerator);

            foreach (IEnumerable<T> other in otherCollections)
            {
                if (other is null)
                {
                    throw new ArgumentNullException(nameof(otherCollections));
                }

                enumerators.Add(other.GetEnumerator());
            }

            while (MoveNextAllEnumerators(enumerators))
            {
                T[] row = enumerators.Select(e => e.Current).ToArray();

                yield return row;
            }
        }
        finally
        {
            foreach (IEnumerator<T> enumerator in enumerators)
            {
                enumerator.Dispose();
            }
        }
    }

    private static bool MoveNextAllEnumerators<T>(ICollection<IEnumerator<T>> enumerators)
    {
        foreach (IEnumerator<T> enumerator in enumerators)
        {
            if (!enumerator.MoveNext())
            {
                return false;
            }
        }

        return true;
    }
}