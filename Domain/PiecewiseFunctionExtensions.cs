using Common;

namespace Domain;

public static class PiecewiseFunctionExtensions
{
	public static (double Min, double Max) GetMinMaxValues(this PiecewiseFunction piecewiseFunction) =>
		piecewiseFunction.Range
			.Split(0.1m)
			.Select(x => piecewiseFunction.Evaluate((double) x))
			.Aggregate((Min: double.MaxValue, Max: double.MinValue), (t, y) => (Math.Min(t.Min, y), Math.Max(t.Max, y)));
}