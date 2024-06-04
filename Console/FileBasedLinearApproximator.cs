using System.Diagnostics;
using Application;
using Domain;
using Intervals.Intervals;

namespace Console;

public class FileBasedLinearApproximator
{
	private static readonly Interval<decimal> LeftInterval = new(-1, 0);
	private static readonly Interval<decimal> MiddlePoint = new(0);
	private static readonly Interval<decimal> RightInterval = new(0, 1, IntervalInclusion.LeftOpened);

	private readonly IExpressionParser _expressionParser;
	private readonly VariationCalculator _variationCalculator;
	private readonly ApproximationBuilder _approximationBuilder;
	private readonly IDistanceEvaluator _distanceEvaluator;

	public FileBasedLinearApproximator(IExpressionParser expressionParser, VariationCalculator variationCalculator,
		ApproximationBuilder approximationBuilder, IDistanceEvaluator distanceEvaluator)
	{
		_expressionParser = expressionParser;
		_variationCalculator = variationCalculator;
		_approximationBuilder = approximationBuilder;
		_distanceEvaluator = distanceEvaluator;
	}

	public void Act()
	{
		using var streamWriter = new StreamWriter("output.txt");

		foreach (var line in File.ReadLines("input.txt"))
		{
			streamWriter.WriteLine($"Прочитано: {line}");
			var stopwatch = Stopwatch.StartNew();

			if (RecogniseLine(line) is var (sourceFunction, variationsRatio))
			{
				streamWriter.WriteLine("Распознано:");

				foreach (var (interval, (_, function)) in sourceFunction.Parts)
					streamWriter.WriteLine($"{interval} {function}");

				var variation = _variationCalculator.GetVariation(sourceFunction);
				streamWriter.WriteLine($"Вариация исходной функции равна {variation}");

				var approximationVariation = variation * variationsRatio;
				streamWriter.WriteLine($"Ищем наилучшее приближение с вариацией, ограниченной {approximationVariation}");

				var approximation = _approximationBuilder.BuildLinearApproximation(sourceFunction, approximationVariation);
				streamWriter.WriteLine("Найдена функция:");

				foreach (var (interval, (_, function)) in approximation.Parts)
					streamWriter.WriteLine($"{interval} {function}");

				streamWriter.WriteLine($"Вариация приближения равна {_variationCalculator.GetVariation(approximation)}");
				streamWriter.WriteLine(
					$"Расстояние между функциями равно {_distanceEvaluator.GetDistance(sourceFunction, approximation)}");
				stopwatch.Stop();
				streamWriter.WriteLine($"Прошло {stopwatch.Elapsed}");
			}
			else
				streamWriter.WriteLine("Не получилось распознать функцию :(");

			streamWriter.WriteLine();
		}
	}

	private (PiecewiseFunction Function, decimal VariationsRatio)? RecogniseLine(string line) =>
		line.Split(' ') is {Length: 6} strings
			? (new PiecewiseFunction(new FunctionPart[]
			{
				new(LeftInterval, _expressionParser.Parse($"{strings[0]}*x+{strings[1]}")),
				new(MiddlePoint, _expressionParser.Parse(strings[2])),
				new(RightInterval, _expressionParser.Parse($"{strings[4]}*x+{strings[3]}")),
			}), decimal.Parse(strings[5]))
			: null;
}