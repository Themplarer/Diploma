using Common;
using Domain;

namespace Application;

public class DistanceEvaluator
{
	public double GetDistance(PiecewiseFunction first, PiecewiseFunction second) =>
		first.Range
			.Split(Constants.StepSize)
			.Max(x => Math.Abs(first.Evaluate((double) x) - second.Evaluate((double) x)));
}