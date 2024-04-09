namespace Common;

public static class EnumerableExtensions
{
	public static IEnumerable<(T First, T Second)> GetBigrams<T>(this IEnumerable<T> items)
	{
		var isFirst = true;
		T? prevValue = default;

		foreach (var item in items)
		{
			if (!isFirst)
				yield return (prevValue!, item);

			isFirst = false;
			prevValue = item;
		}
	}

	public static IEnumerable<(T First, T Second)> GetPairs<T>(this IReadOnlyCollection<T> items)
	{
		foreach (var (first, index) in items.Enumerate())
		foreach (var second in items.Skip(index + 1))
			yield return (first, second);
	}

	public static IEnumerable<(T Element, int Index)> Enumerate<T>(this IEnumerable<T> items) =>
		items.Select((t, i) => (t, i));
}