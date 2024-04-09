using System.Numerics;
using Intervals.Intervals;

namespace Common;

public static class IntervalExtensions
{
	public static IEnumerable<T> Split<T>(this Interval<T> interval, T step)
		where T : IComparable<T>, IEquatable<T>, IAdditionOperators<T, T, T>, IComparisonOperators<T, T, bool>
	{
		for (var x = interval.LeftValue; x <= interval.RightValue; x += step)
			if (interval.IsInclude(new Interval<T>(x)))
				yield return x;
	}
}