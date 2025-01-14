﻿namespace Common;

public static class EnumerableExtensions
{
	public static IEnumerable<(T First, T Second)> GetBigrams<T>(this IEnumerable<T> items) =>
		items.GetBigramsWithEndingNull()
			.SkipLast(1)
			.Cast<(T, T)>();

	public static IEnumerable<(T First, T? Second)> GetBigramsWithEndingNull<T>(this IEnumerable<T> items)
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

		yield return (prevValue!, default);
	}

	public static IEnumerable<(T Element, int Index)> Enumerate<T>(this IEnumerable<T> items) =>
		items.Select((t, i) => (t, i));
}