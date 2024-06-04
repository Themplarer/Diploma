using System.Globalization;
using Common;
using Domain;

namespace Application.ApproximationBuilders;

internal sealed class SingleDiscontinuityApproximationBuilder : IApproximationBuilder
{
	public bool IsFallback => false;

	public PiecewiseFunction? Build(PiecewiseFunction sourceFunction, decimal variation, decimal newVariation)
	{
		if (sourceFunction.GetDiscontinuities().Cast<decimal?>().SingleOrDefault() is not { } discontinuity)
			return null;

		var (leftPart, leftIndex) =
			sourceFunction.Parts.Enumerate().First(p => p.Element.Interval.RightValue == discontinuity);
		var (rightPart, rightIndex) =
			sourceFunction.Parts.Enumerate().Last(p => p.Element.Interval.LeftValue == discontinuity);

		var leftLimit = leftPart.Function.Method(discontinuity);
		var exactValue = sourceFunction.Evaluate(discontinuity)!.Value;
		var rightLimit = rightPart.Function.Method(discontinuity);
		var leftDiff = Math.Abs(leftLimit - exactValue);
		var rightDiff = Math.Abs(rightLimit - exactValue);
		var totalDiff = Math.Abs(leftLimit - rightLimit);

		if (variation - leftDiff - rightDiff > newVariation)
			return null;

		var midpoint = GetMidpoint(leftLimit, exactValue, rightLimit, leftDiff, rightDiff, totalDiff);
		return new PiecewiseFunction(
			sourceFunction.Parts.Enumerate()
				.Select((t, i) =>
				{
					if (i <= leftIndex)
					{
						var diff = midpoint - leftLimit;
						return t.Element with
						{
							Function = new RepresentableFunction(x => t.Element.Function.Method(x) + diff,
								$"{t.Element.Function.Representation} + {diff}")
						};
					}

					if (i >= rightIndex)
					{
						var diff = midpoint - rightLimit;
						return t.Element with
						{
							Function = new RepresentableFunction(x => t.Element.Function.Method(x) + diff,
								$"{t.Element.Function.Representation} + {diff}")
						};
					}

					return t.Element with
					{
						Function = new RepresentableFunction(_ => midpoint, midpoint.ToString(CultureInfo.InvariantCulture))
					};
				})
				.ToArray());
	}

	private static decimal GetMidpoint(decimal leftLimit, decimal exactValue, decimal rightLimit, decimal leftDiff,
		decimal rightDiff, decimal totalDiff)
	{
		if (leftDiff >= rightDiff && leftDiff >= totalDiff)
			return (leftLimit + exactValue) / 2;

		if (rightDiff >= leftDiff && rightDiff >= totalDiff)
			return (rightLimit + exactValue) / 2;

		return (leftLimit + rightLimit) / 2;
	}
}