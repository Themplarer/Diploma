using Common;
using Domain;

namespace Application.DistanceEvaluators;

internal sealed class UniformDistanceEvaluator : IDistanceEvaluator
{
	public decimal GetDistance(IFunction first, IFunction second) =>
		first.Range
			.Split(Constants.StepSize)
			.Max(x => Math.Abs(first.Evaluate(x)!.Value - second.Evaluate(x)!.Value));
}