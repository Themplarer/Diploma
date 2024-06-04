using Intervals.Intervals;

namespace Domain;

public interface IFunction
{
	decimal? Evaluate(decimal argument);

	Interval<decimal> Range { get; }
}