using Common;

namespace Domain;

public static class PiecewiseFunctionExtensions
{
	private const decimal Step = 0.1m;

	public static (decimal Min, decimal Max) GetMinMaxValues(this PiecewiseFunction piecewiseFunction) =>
		piecewiseFunction.Range
			.Split(Step)
			.Select(piecewiseFunction.Evaluate)
			.Cast<decimal>()
			.Aggregate((Min: decimal.MaxValue, Max: decimal.MinValue), (t, y) => (Math.Min(t.Min, y), Math.Max(t.Max, y)));

	public static bool IsContinuous(this PiecewiseFunction piecewiseFunction) =>
		piecewiseFunction.GetDiscontinuities().Count == 0;

	public static IReadOnlyCollection<decimal> GetDiscontinuities(this PiecewiseFunction piecewiseFunction) =>
		piecewiseFunction
			.Parts
			.GetBigrams()
			.Where(t =>
			{
				var commonPoint = t.First.Interval.RightValue;
				return t.First.Function.Method(commonPoint) != t.Second.Function.Method(commonPoint);
			})
			.Select(t => t.First.Interval.RightValue)
			.ToHashSet();
}