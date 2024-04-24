using Intervals.Intervals;
using ScottPlot;

namespace Gui.Extensions;

internal static class AxisManagerExtensions
{
	public static void SetLimits(this AxisManager axisManager, Interval<decimal> range)
	{
		var (left, right, _) = range;
		axisManager.SetLimitsX((double) left, (double) right);
	}
}