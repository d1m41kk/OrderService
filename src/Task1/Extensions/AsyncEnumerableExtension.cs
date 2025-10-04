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

            while (true)
            {
                foreach (IAsyncEnumerator<T> t in enumerators)
                {
                    if (!await t.MoveNextAsync())
                    {
                        yield break;
                    }
                }

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
}