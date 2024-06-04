using Common;
using Domain;

namespace Application;

public class VariationCalculator
{
	public decimal GetVariation(FunctionPart functionPart) => GetVariation(new PiecewiseFunction(new[] {functionPart}));

	public decimal GetVariation(PiecewiseFunction piecewiseFunction) =>
		piecewiseFunction.Parts
			.SelectMany(GetExtremes, (f, x) => f.Function.Method(x))
			.GetBigrams()
			.Sum(t => Math.Abs(t.First - t.Second));

	private static IEnumerable<decimal> GetExtremes(FunctionPart functionPart)
	{
		Monotonicity GetMonotonicity(((decimal X, decimal Y) First, (decimal X, decimal Y) Second) pair) =>
			pair.Second is (0, 0)
				? Monotonicity.Unknown
				: pair.First.Y.CompareTo(pair.Second.Y) switch
				{
					> 0 => Monotonicity.Decreasing,
					0 => Monotonicity.Constant,
					< 0 => Monotonicity.Increasing
				};

		var (interval, (function, _)) = functionPart;

		return interval.Close()
			.Split(Constants.StepSize)
			.Select(x => (X: x, Y: function(x)))
			.GetBigramsWithEndingNull()
			.Select(t => (t.First.X, Monotonicity: GetMonotonicity(t)))
			.Aggregate((CurrentMonotonicity: Monotonicity.Unknown, Extremes: new List<decimal>()),
				(state, t) => (t.Monotonicity, state.Extremes.AddIf(t.X, state.CurrentMonotonicity != t.Monotonicity)))
			.Extremes;
	}

	private enum Monotonicity
	{
		Unknown,
		Decreasing,
		Constant,
		Increasing
	}
}