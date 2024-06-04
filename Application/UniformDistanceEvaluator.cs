using Common;
using Domain;

namespace Application;

internal sealed class UniformDistanceEvaluator : IDistanceEvaluator
{
	public decimal GetDistance(PiecewiseFunction first, PiecewiseFunction second) =>
		first.Range
			.Split(Constants.StepSize)
			.Max(x => Math.Abs(first.Evaluate(x)!.Value - second.Evaluate(x)!.Value));
}