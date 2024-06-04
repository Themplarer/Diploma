using System.Numerics;
using Intervals.Intervals;
using Intervals.Points;

namespace Common;

public static class IntervalExtensions
{
	public static IEnumerable<T> Split<T>(this Interval<T> interval, T step)
		where T : IComparable<T>, IEquatable<T>, IAdditionOperators<T, T, T>, IComparisonOperators<T, T, bool>
	{
		for (var x = interval.LeftValue; x <= interval.RightValue; x += step)
			if (interval.IsInclude(x))
				yield return x;
	}

	public static Interval<T> Close<T>(this Interval<T> interval)
		where T : IComparable<T>, IEquatable<T> =>
		new(interval.LeftValue, interval.RightValue, IntervalInclusion.Closed);

	public static bool IsInclude<T>(this Interval<T> interval, T value)
		where T : IComparable<T>, IEquatable<T> =>
		interval.LeftValue.CompareTo(value) < 0 && value.CompareTo(interval.RightValue) < 0 ||
		interval.Left == new Endpoint<T>(Point.Included(value), EndpointLocation.Left) ||
		interval.Right == new Endpoint<T>(Point.Included(value), EndpointLocation.Right);
}