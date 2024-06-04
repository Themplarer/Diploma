using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Application;
using Application.ApproximationBuilders;
using Application.DistanceEvaluators;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Domain;
using Gui.Extensions;
using MsBox.Avalonia;
using ScottPlot;

namespace Gui.Views;

public partial class MainWindow : Window
{
	private readonly IExpressionParser _expressionParser;
	private readonly VariationCalculator _variationCalculator;
	private readonly IDistanceEvaluator _distanceEvaluator;
	private readonly IApproximationBuilder[] _approximationBuilders;

	public MainWindow(IExpressionParser expressionParser, VariationCalculator variationCalculator,
		IDistanceEvaluator distanceEvaluator, IEnumerable<IApproximationBuilder> approximationBuilders)
	{
		_expressionParser = expressionParser;
		_variationCalculator = variationCalculator;
		_distanceEvaluator = distanceEvaluator;
		_approximationBuilders = approximationBuilders.ToArray();

		InitializeComponent();

		SourceFunctionBlock.Changed += DrawSourceFunction;
	}

	[SuppressMessage("ReSharper", "UnusedParameter.Local")]
	private async void UpdateApproximationAsync(object? sender, RoutedEventArgs e)
	{
		if (GetInputFunction() is not { } sourceFunction)
			return;

		var sourceVariation = SetSourceVariation(sourceFunction);

		if (await BuildApproximationWithTimeTrackingAsync(sourceFunction, sourceVariation) is not { } approximationFunction)
			return;

		FillApproximation(approximationFunction);
		FillDistance(sourceFunction, approximationFunction);
		UpdatePlot(sourceFunction, approximationFunction);
	}

	private void DrawSourceFunction(object? sender, EventArgs e)
	{
		ApproximationFunctionBlock.IsVisible = false;

		if (GetInputFunction() is { } sourceFunction)
		{
			SetSourceVariation(sourceFunction);
			DistanceBlock.IsVisible = false;
			UpdatePlot(sourceFunction);
		}
		else
		{
			SourceFunctionBlock.HideVariation();
			Plot.Plot.Clear();
			Plot.Refresh();
		}
	}

	private PiecewiseFunction? GetInputFunction()
	{
		try
		{
			var sourceFunction = SourceFunctionBlock.ParseFunction(_expressionParser);

			if (!sourceFunction.IsCorrect)
			{
				SetStatusBarText(
					"Область определения функции некорректна: например, пересекаются отрезки определения кусочной функции, или они не связны");
				return null;
			}

			return sourceFunction;
		}
		catch
		{
			SetStatusBarText("Не удалось распознать введённую функцию");
			return null;
		}
	}

	private async Task<PiecewiseFunction?> BuildApproximationWithTimeTrackingAsync(PiecewiseFunction sourceFunction,
		decimal sourceVariation)
	{
		var stopwatch = Stopwatch.StartNew();
		var approximation = await BuildApproximationAsync(sourceFunction, sourceVariation);
		stopwatch.Stop();

		SetStatusBarText(approximation is { }
			? $"Приближение найдено за {stopwatch}"
			: "Не удалось построить приближение функции");

		return approximation;
	}

	private async Task<PiecewiseFunction?> BuildApproximationAsync(PiecewiseFunction sourceFunction,
		decimal sourceVariation)
	{
		var newVariation = ByRatio.IsChecked is true
			? sourceVariation * VariationsRatio.Value!.Value
			: NewVariation.Value!.Value;

		if (_approximationBuilders
			    .TakeWhile(b => !b.IsFallback)
			    .Select(b => b.Build(sourceFunction, sourceVariation, newVariation))
			    .FirstOrDefault(f => f is not null) is { } approximation)
			return approximation;

		await WarnForNoExactApproximationsAsync();
		return _approximationBuilders
			.SkipWhile(b => !b.IsFallback)
			.Select(b => b.Build(sourceFunction, sourceVariation, newVariation))
			.FirstOrDefault(f => f is not null);
	}

	private void FillApproximation(PiecewiseFunction approximationFunction)
	{
		ApproximationFunctionBlock.FillParts(approximationFunction);
		ApproximationFunctionBlock.SetVariation(_variationCalculator.GetVariation(approximationFunction));
		ApproximationFunctionBlock.IsVisible = true;
	}

	private void FillDistance(PiecewiseFunction sourceFunction, PiecewiseFunction approximationFunction)
	{
		// ReSharper disable once SpecifyACultureInStringConversionExplicitly
		Distance.Text = _distanceEvaluator.GetDistance(sourceFunction, approximationFunction).ToString();
		DistanceBlock.IsVisible = true;
	}

	private decimal SetSourceVariation(PiecewiseFunction sourceFunction)
	{
		var sourceVariation = _variationCalculator.GetVariation(sourceFunction);
		SourceFunctionBlock.SetVariation(sourceVariation);
		return sourceVariation;
	}

	private void UpdatePlot(PiecewiseFunction sourceFunction, PiecewiseFunction? approximationFunction = null)
	{
		Plot.Plot.Clear();
		Plot.Plot.Axes.SquareUnits();
		Plot.Plot.Axes.SetLimits(sourceFunction.Range);
		Plot.Plot.Add.ScatterPiecewiseFunction(sourceFunction, Colors.DarkCyan, 2);

		if (approximationFunction is not null)
			Plot.Plot.Add.ScatterPiecewiseFunction(approximationFunction, Colors.Red, 1);

		Plot.Refresh();
	}

	private void SetStatusBarText(string statusBarText) => StatusBar.Text = statusBarText;

	private static async Task WarnForNoExactApproximationsAsync() =>
		await MessageBoxManager.GetMessageBoxStandard("Предупреждение",
				"Не удалось построить точное приближение для данной функции. Для построения приближения будет выполнен перебор. Это может занять несколько минут.")
			.ShowAsync();
}