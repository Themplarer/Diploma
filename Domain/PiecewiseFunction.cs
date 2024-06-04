using Common;
using Intervals.Intervals;

namespace Domain;

public record PiecewiseFunction(FunctionPart[] Parts) : IFunction
{
	public Interval<decimal> Range => TotalRange.Single();

	public bool IsCorrect =>
		Parts.All(f => !f.Interval.IsEmpty()) && TotalRange.Length is 1 &&
		Parts
			.Select(f => f.Interval)
			.Order()
			.GetBigrams()
			.All(t => !t.First.IsOverlap(t.Second));

	public decimal? Evaluate(decimal argument)
	{
		var (_, (function, _)) = Parts.FirstOrDefault(f => f.Interval.IsInclude(argument));
		return function?.Invoke(argument);
	}

	private Interval<decimal>[] TotalRange => Parts.Select(f => f.Interval).Combine().ToArray();
}