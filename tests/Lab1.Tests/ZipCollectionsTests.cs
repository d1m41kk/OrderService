using Task1.Extensions;
using Xunit;

namespace Lab1.Tests;

public class ZipCollectionsTests
{
    public static IEnumerable<object[]> EqualLengthCases()
    {
        yield return MakeEqualLengthCase(3, 0);
        yield return MakeEqualLengthCase(3, 1);
        yield return MakeEqualLengthCase(4, 2);
        yield return MakeEqualLengthCase(2, 5);
    }

    public static IEnumerable<object[]> UnequalLengthCases()
    {
        yield return MakeUnequalLengthCase(5, [3]);
        yield return MakeUnequalLengthCase(4, [6, 2]);
        yield return MakeUnequalLengthCase(7, [7, 5, 9]);
    }

    [Fact]
    public void Scenario1_Sync_Zip_NoArgs_ReturnsSingletons()
    {
        int[] source = [10, 20, 30];

        var rows = source.ZipCollections().ToList();

        Assert.Equal(source.Length, rows.Count);
        foreach ((int[] row, int i) in rows.Select((r, i) => (r, i)))
        {
            Assert.Single(row);
            Assert.Equal(source[i], row[0]);
        }
    }

    [Fact]
    public async Task Scenario1_Async_Zip_NoArgs_ReturnsSingletons()
    {
        string[] source = ["a", "b", "c"];
        IAsyncEnumerable<string> asyncSource = ToAsync(source);

        List<string[]> rows = await CollectAsync(asyncSource.AsyncZipCollections());

        Assert.Equal(source.Length, rows.Count);
        foreach ((string[] row, int i) in rows.Select((r, i) => (r, i)))
        {
            Assert.Single(row);
            Assert.Equal(source[i], row[0]);
        }
    }

    [Theory]
    [MemberData(nameof(EqualLengthCases))]
    public void Scenario2_Sync_Zip_VariousEqualLengths(
        IEnumerable<int> first,
        IEnumerable<int>[] others,
        int n,
        int k)
    {
        var rows = first.ZipCollections(others).ToList();

        Assert.Equal(n, rows.Count);
        foreach (int[] row in rows)
            Assert.Equal(1 + k, row.Length);

        for (int i = 0; i < n; i++)
        {
            int[] expected = ExpectedRowEqualLength(i + 1, k);
            Assert.Equal(expected, rows[i]);
        }
    }

    [Theory]
    [MemberData(nameof(EqualLengthCases))]
    public async Task Scenario2_Async_Zip_VariousEqualLengths(
        IEnumerable<int> first,
        IEnumerable<int>[] others,
        int n,
        int k)
    {
        IAsyncEnumerable<int> firstA = ToAsync(first);
        IAsyncEnumerable<int>[] othersA = others.Select(ToAsync).ToArray();

        List<int[]> rows = await CollectAsync(firstA.AsyncZipCollections(othersA));

        Assert.Equal(n, rows.Count);
        foreach (int[] row in rows)
            Assert.Equal(1 + k, row.Length);

        for (int i = 0; i < n; i++)
        {
            int[] expected = ExpectedRowEqualLength(i + 1, k);
            Assert.Equal(expected, rows[i]);
        }
    }

    [Theory]
    [MemberData(nameof(UnequalLengthCases))]
    public void Scenario3_Sync_Zip_VariousUnequalLengths(
        IEnumerable<int> first,
        IEnumerable<int>[] others,
        int expectedCount)
    {
        IEnumerable<int> enumerable = first as int[] ?? first.ToArray();
        var rows = enumerable.ZipCollections(others).ToList();

        Assert.Equal(expectedCount, rows.Count);
        foreach (int[] row in rows)
            Assert.Equal(1 + others.Length, row.Length);

        for (int i = 0; i < expectedCount; i++)
        {
            Assert.Equal(enumerable.ElementAt(i), rows[i][0]);
            for (int j = 0; j < others.Length; j++)
                Assert.Equal(others[j].ElementAt(i), rows[i][1 + j]);
        }
    }

    [Theory]
    [MemberData(nameof(UnequalLengthCases))]
    public async Task Scenario3_Async_Zip_VariousUnequalLengths(
        IEnumerable<int> first,
        IEnumerable<int>[] others,
        int expectedCount)
    {
        IEnumerable<int> enumerable = first as int[] ?? first.ToArray();
        IAsyncEnumerable<int> firstA = ToAsync(enumerable);
        IAsyncEnumerable<int>[] othersA = others.Select(ToAsync).ToArray();

        List<int[]> rows = await CollectAsync(firstA.AsyncZipCollections(othersA));

        Assert.Equal(expectedCount, rows.Count);
        foreach (int[] row in rows)
            Assert.Equal(1 + others.Length, row.Length);

        for (int i = 0; i < expectedCount; i++)
        {
            Assert.Equal(enumerable.ElementAt(i), rows[i][0]);
            for (int j = 0; j < others.Length; j++)
                Assert.Equal(others[j].ElementAt(i), rows[i][1 + j]);
        }
    }

    private static object[] MakeUnequalLengthCase(int firstLen, int[] othersLens)
    {
        int[] first = Enumerable.Range(1, firstLen).ToArray();
        IEnumerable<int>[] others = othersLens
            .Select((len, j) =>
                Enumerable.Range(1, len).Select(x => j + 1 + (x * 10)))
            .ToArray();

        int expectedCount = new[] { firstLen }.Concat(othersLens).Min();
        return [first, others, expectedCount];
    }

    private static async Task<List<T[]>> CollectAsync<T>(IAsyncEnumerable<T[]> seq)
    {
        var list = new List<T[]>();
        await foreach (T[] item in seq)
            list.Add(item);
        return list;
    }

    private static async IAsyncEnumerable<T> ToAsync<T>(IEnumerable<T> src)
    {
        foreach (T item in src)
        {
            await Task.Yield();
            yield return item;
        }
    }

    private static object[] MakeEqualLengthCase(int n, int k)
    {
        int[] first = Enumerable.Range(1, n).ToArray();
        IEnumerable<int>[] others = Enumerable.Range(0, k)
            .Select(j => first.Select(x => j + 1 + (x * 10)))
            .ToArray();

        return [first, others, n, k];
    }

    private static int[] ExpectedRowEqualLength(int baseValue, int k)
    {
        int[] row = new int[1 + k];
        row[0] = baseValue;
        for (int j = 0; j < k; j++)
            row[1 + j] = (baseValue * 10) + j + 1;
        return row;
    }
}