using Common;
using Domain;

namespace Application;

public class VariationCalculator
{
	public double GetVariation(FunctionPart functionPart) => GetVariation(new PiecewiseFunction(new[] {functionPart}));

	public double GetVariation(PiecewiseFunction piecewiseFunction) =>
		piecewiseFunction.Parts
			.SelectMany(GetExtremes, (f, x) => f.Function.Method((double) x))
			.GetBigrams()
			.Sum(t => Math.Abs(t.First - t.Second));

	private IEnumerable<decimal> GetExtremes(FunctionPart functionPart)
	{
		var (interval, (function, _)) = functionPart;

		var prevArgument = interval.LeftValue;
		yield return interval.LeftValue;

		if (!interval.IsEmpty())
		{
			var prevValue = function((double) interval.LeftValue);
			var isIncreasing = function((double) (interval.LeftValue + Constants.StepSize)) > prevValue;

			foreach (var x in interval.Split(Constants.StepSize))
			{
				var value = function((double) x);

				if (isIncreasing)
				{
					if (prevValue.CompareTo(value) > 0)
						yield return prevArgument;
				}
				else
				{
					if (prevValue.CompareTo(value) < 0)
						yield return prevArgument;
				}

				prevValue = value;
				prevArgument = x;
			}
		}

		yield return interval.RightValue;
	}
}