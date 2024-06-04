using Common;
using Domain;
using Intervals.Intervals;

namespace Application;

public class ApproximationBuilder
{
	private readonly VariationCalculator _variationCalculator;
	private readonly IDistanceEvaluator _distanceEvaluator;
	private readonly IExpressionParser _expressionParser;

	public ApproximationBuilder(VariationCalculator variationCalculator, IDistanceEvaluator distanceEvaluator,
		IExpressionParser expressionParser)
	{
		_variationCalculator = variationCalculator;
		_distanceEvaluator = distanceEvaluator;
		_expressionParser = expressionParser;
	}

	public PiecewiseFunction BuildLinearApproximation(PiecewiseFunction sourceFunction, decimal newVariation) =>
		GenerateLinearApproximations(sourceFunction, newVariation)
			.MinBy(f => _distanceEvaluator.GetDistance(sourceFunction, f))!;

	private IEnumerable<PiecewiseFunction> GenerateLinearApproximations(PiecewiseFunction piecewiseFunction,
		decimal newVariation)
	{
		return Build(new FunctionPart[piecewiseFunction.Parts.Length], 0, 0);

		Interval<decimal> GetFreeParameterRange()
		{
			var (min, max) = piecewiseFunction.GetMinMaxValues();
			return new Interval<decimal>(min, max, IntervalInclusion.Closed);
		}

		IEnumerable<PiecewiseFunction> Build(FunctionPart[] functionParts, decimal currentVariation, int index)
		{
			if (index == piecewiseFunction.Parts.Length)
				yield return new PiecewiseFunction(functionParts);
			else if (index == 0)
				foreach (var b in GetFreeParameterRange().Split(Constants.ApproximationStepSize))
				foreach (var function in GetVariants(functionParts, currentVariation, index,
					         piecewiseFunction.Parts[0].Interval.LeftValue, b))
					yield return function;
			else
			{
				var (interval, (func, _)) = functionParts[index - 1];
				var intervalStart = interval.RightValue;

				foreach (var function in
				         GetVariants(functionParts, currentVariation, index, intervalStart, func(intervalStart)))
					yield return function;
			}
		}

		IEnumerable<PiecewiseFunction> GetVariants(IReadOnlyList<FunctionPart> functionParts, decimal currentVariation,
			int index, decimal startX, decimal startY)
		{
			var paramInterval = piecewiseFunction.Parts[index].Interval.GetDiff<decimal, decimal>() > 0 &&
			                    newVariation - currentVariation is var remainingVariation
				? new Interval<decimal>(-remainingVariation, remainingVariation, IntervalInclusion.Closed)
				: new Interval<decimal>(0);

			foreach (var k in paramInterval.Split(Constants.ApproximationStepSize))
			{
				var newParts = functionParts.ToArray();
				newParts[index] = piecewiseFunction.Parts[index] with
				{
					Function = new RepresentableFunction(x => (x - startX) * k + startY,
						$"(x - {startX}) * {k} + {startY}")
				};

				foreach (var function in Build(newParts, currentVariation + Math.Abs(k), index + 1))
					yield return function;
			}
		}
	}
}