using Common;
using Intervals.Intervals;

namespace Domain;

public record PiecewiseFunction(FunctionPart[] Parts)
{
	public Interval<decimal> Range => TotalRange.Single();

	public bool IsCorrect =>
		Parts.All(f => !f.Interval.IsEmpty()) && TotalRange.Count() == 1 &&
		Parts
			.Select(f => f.Interval)
			.Order()
			.GetBigrams()
			.All(t => !t.First.IsOverlap(t.Second));

	public double Evaluate(double argument)
	{
		var (_, (function, _)) = Parts.FirstOrDefault(f => f.Interval.IsInclude(new Interval<decimal>((decimal) argument)));
		return function?.Invoke(argument) ?? 0;
	}

	private IEnumerable<Interval<decimal>> TotalRange => Parts.Select(f => f.Interval).Combine();
}