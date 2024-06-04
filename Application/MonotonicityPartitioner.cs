using Common;
using Domain;
using Intervals.Intervals;

namespace Application;

public class MonotonicityPartitioner
{
	public IEnumerable<Interval<decimal>> BuildPartition(PiecewiseFunction piecewiseFunction)
	{
		var range = piecewiseFunction.Range;

		Monotonicity GetMonotonicity(decimal? firstY, decimal? secondY) =>
			firstY is null || secondY is null
				? Monotonicity.Unknown
				: firstY.Value.CompareTo(secondY) switch
				{
					> 0 => Monotonicity.Decreasing,
					0 => Monotonicity.Constant,
					< 0 => Monotonicity.Increasing
				};

		return piecewiseFunction.Range.Close()
			.Split(Constants.StepSize)
			.Select(x => (X: x, Y: piecewiseFunction.Evaluate(x)))
			.GetBigramsWithEndingNull()
			.Select(t => (t.First.X, Monotonicity: GetMonotonicity(t.First.Y, t.Second.Y)))
			.Aggregate((CurrentMonotonicity: Monotonicity.Unknown, Start: 0m, Partition: new List<Interval<decimal>>()),
				(state, t) =>
				{
					if (state.CurrentMonotonicity is Monotonicity.Unknown or Monotonicity.Constant)
						return (t.Monotonicity, t.X, state.Partition);

					if (t.Monotonicity != Monotonicity.Constant && state.CurrentMonotonicity != t.Monotonicity)
					{
						state.Partition.Add(new Interval<decimal>(state.Start, t.X, IntervalInclusion.Closed));
						return (t.Monotonicity, t.X, state.Partition);
					}

					return (state.CurrentMonotonicity, state.Start, state.Partition);
				})
			.Partition
			.Select(t => t.Overlap(range).SingleOrDefault())
			.Where(t => t is not null)!;
	}

	private enum Monotonicity
	{
		Unknown,
		Decreasing,
		Constant,
		Increasing
	}
}