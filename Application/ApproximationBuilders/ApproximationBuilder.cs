using Application.DistanceEvaluators;
using Common;
using Domain;
using Intervals.Intervals;

namespace Application.ApproximationBuilders;

internal sealed class ApproximationBuilder : IApproximationBuilder
{
	private readonly VariationCalculator _variationCalculator;
	private readonly IDistanceEvaluator _distanceEvaluator;

	public ApproximationBuilder(VariationCalculator variationCalculator, IDistanceEvaluator distanceEvaluator)
	{
		_variationCalculator = variationCalculator;
		_distanceEvaluator = distanceEvaluator;
	}

	public bool IsFallback => true;

	public PiecewiseFunction? Build(PiecewiseFunction sourceFunction, decimal variation, decimal newVariation) =>
		GenerateApproximations(sourceFunction, newVariation)
			.MinBy(f => _distanceEvaluator.GetDistance(sourceFunction, f)) is {Backbone: not null} piecewiseLinearCombination
			? piecewiseLinearCombination.Materialize()
			: null;

	private IEnumerable<PiecewiseLinearCombination> GenerateApproximations(PiecewiseFunction piecewiseFunction,
		decimal newVariation)
	{
		var variations = piecewiseFunction.Parts.Select(_variationCalculator.GetVariation).ToArray();
		var parameters = new (decimal Multiplier, decimal Offset)[variations.Length];
		return Build1(new PiecewiseLinearCombination(piecewiseFunction, parameters), 0, 0);

		Interval<decimal> GetFreeParameterRange()
		{
			var (min, max) = piecewiseFunction.GetMinMaxValues();
			return new Interval<decimal>(min, max, IntervalInclusion.Closed);
		}

		IEnumerable<PiecewiseLinearCombination> Build1(PiecewiseLinearCombination piecewiseLinearCombination,
			decimal currentVariation, int index)
		{
			if (index == piecewiseFunction.Parts.Length)
				yield return piecewiseLinearCombination;
			else if (index == 0)
				foreach (var startY in GetFreeParameterRange().Split(Constants.ApproximationStepSize))
				foreach (var function in GetVariants(piecewiseLinearCombination, currentVariation, index, startY))
					yield return function;
			else
			{
				var intervalStart = piecewiseFunction.Parts[index].Interval.LeftValue;
				var startY = piecewiseLinearCombination.Evaluate(intervalStart)!.Value;

				foreach (var function in GetVariants(piecewiseLinearCombination, currentVariation, index, startY))
					yield return function;
			}
		}

		IEnumerable<PiecewiseLinearCombination> GetVariants(PiecewiseLinearCombination piecewiseLinearCombination,
			decimal currentVariation, int index, decimal startY)
		{
			var (interval, (method, _)) = piecewiseFunction.Parts[index];
			var variationsInterval = interval.GetDiff<decimal, decimal>() > 0 &&
			                         newVariation - currentVariation is var remainingVariation
				? new Interval<decimal>(-remainingVariation, remainingVariation, IntervalInclusion.Closed)
				: new Interval<decimal>(0);
			var pieceVariation = variations[index];
			var rightLimit = method(interval.LeftValue);

			foreach (var variation in variationsInterval.Split(Constants.ApproximationStepSize))
			{
				var multiplier = pieceVariation is 0 ? 0 : variation / pieceVariation;
				var clone = piecewiseLinearCombination with {Parameters = piecewiseLinearCombination.Parameters.ToArray()};
				clone.Parameters[index] = (multiplier, startY - rightLimit * multiplier);

				foreach (var function in Build1(clone, currentVariation + Math.Abs(variation), index + 1))
					yield return function;
			}
		}
	}

	private readonly record struct PiecewiseLinearCombination(PiecewiseFunction Backbone,
		(decimal Multiplier, decimal Offset)[] Parameters) : IFunction
	{
		public decimal? Evaluate(decimal argument) =>
			Backbone.Parts
					.Zip(Parameters)
					.FirstOrDefault(t => t.First.Interval.IsInclude(argument))
				is ({Interval: not null} functionPart, var (multiplier, offset))
				? functionPart.Function.Method(argument) * multiplier + offset
				: null;

		public Interval<decimal> Range => Backbone.Range;

		public PiecewiseFunction Materialize() =>
			new(Backbone.Parts
				.Zip(Parameters, (part, tuple) =>
				{
					var (interval, (method, representation)) = part;
					var (multiplier, offset) = tuple;
					return new FunctionPart(interval, new RepresentableFunction(x => multiplier * method(x) + offset,
						$"{multiplier} * ({representation}) + {offset}"));
				})
				.ToArray());
	}
}