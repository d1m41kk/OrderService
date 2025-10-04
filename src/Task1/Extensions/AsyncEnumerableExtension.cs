namespace Task1.Extensions;

public static class AsyncEnumerableExtension
{
    public static async IAsyncEnumerable<T[]> AsyncZipCollections<T>(
        this IAsyncEnumerable<T> firstCollection,
        params IAsyncEnumerable<T>[] otherCollections)
    {
        ArgumentNullException.ThrowIfNull(firstCollection);
        ArgumentNullException.ThrowIfNull(otherCollections);
        var enumerators = new List<IAsyncEnumerator<T>>(1 + otherCollections.Length);

        try
        {
            IAsyncEnumerator<T> enumerator = firstCollection.GetAsyncEnumerator();
            enumerators.Add(enumerator);

            foreach (IAsyncEnumerable<T>? other in otherCollections)
            {
                if (other is null)
                {
                    throw new ArgumentNullException(nameof(otherCollections));
                }

                enumerators.Add(other.GetAsyncEnumerator());
            }

            while (await MoveNextAllEnumerators(enumerators))
            {
                T[] arrays = enumerators.Select(e => e.Current).ToArray();

                yield return arrays;
            }
        }
        finally
        {
            foreach (IAsyncEnumerator<T> enumerator in enumerators)
            {
                if (enumerator is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
            }
        }
    }

    private static async ValueTask<bool> MoveNextAllEnumerators<T>(ICollection<IAsyncEnumerator<T>> enumerators)
    {
        foreach (IAsyncEnumerator<T> enumerator in enumerators)
        {
            if (!await enumerator.MoveNextAsync())
            {
                return false;
            }
        }

        return true;
    }
}