using Common;
using Domain;
using Intervals.Intervals;

namespace Application;

public class ApproximationBuilder
{
	private readonly VariationCalculator _variationCalculator;
	private readonly DistanceEvaluator _distanceEvaluator;
	private readonly ExpressionParser _expressionParser;

	public ApproximationBuilder(VariationCalculator variationCalculator, DistanceEvaluator distanceEvaluator,
		ExpressionParser expressionParser)
	{
		_variationCalculator = variationCalculator;
		_distanceEvaluator = distanceEvaluator;
		_expressionParser = expressionParser;
	}

	public PiecewiseFunction BuildLinearApproximation(PiecewiseFunction sourceFunction, double newVariation) =>
		GenerateLinearApproximations(sourceFunction, newVariation)
			.MinBy(f => _distanceEvaluator.GetDistance(sourceFunction, f))!;

	private IEnumerable<PiecewiseFunction> GenerateLinearApproximations(PiecewiseFunction piecewiseFunction,
		double newVariation)
	{
		var freeParamInterval =
			new Interval<decimal>((decimal) -newVariation, (decimal) newVariation, IntervalInclusion.Closed);

		return Build(new FunctionPart[piecewiseFunction.Parts.Length], 0);

		IEnumerable<PiecewiseFunction> Build(FunctionPart[] functionParts, int n)
		{
			if (piecewiseFunction.Parts.Length == n)
			{
				yield return new PiecewiseFunction(functionParts);
			}
			else if (n == 0)
			{
				foreach (var b in freeParamInterval.Split(Constants.ApproximationStepSize))
				foreach (var k in freeParamInterval.Split(Constants.ApproximationStepSize))
				{
					var newParts = functionParts.ToArray();
					var (left, _, _) = piecewiseFunction.Parts[0].Interval;
					newParts[0] = piecewiseFunction.Parts[0] with
					{
						Function = _expressionParser.Parse($"(x - {left}) * {k} + {b}")
					};

					if (_variationCalculator.GetVariation(new PiecewiseFunction(newParts[..1])) <= newVariation)
						foreach (var function in Build(newParts, n + 1))
							yield return function;
				}
			}
			else
			{
				foreach (var k in freeParamInterval.Split(Constants.ApproximationStepSize))
				{
					var newParts = functionParts.ToArray();
					var ((_, right, _), (func, _)) = newParts[n - 1];
					newParts[n] = piecewiseFunction.Parts[n] with
					{
						Function = _expressionParser.Parse($"(x - {right}) * {k} + {func((double) right)}")
					};

					if (_variationCalculator.GetVariation(new PiecewiseFunction(newParts[..(n + 1)])) <= newVariation)
						foreach (var function in Build(newParts, n + 1))
							yield return function;
				}
			}
		}
	}
}