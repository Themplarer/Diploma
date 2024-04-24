using Application;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Domain;
using Gui.Extensions;
using ScottPlot;

namespace Gui.Views;

public partial class MainWindow : Window
{
	private readonly ExpressionParser _expressionParser;
	private readonly VariationCalculator _variationCalculator;
	private readonly DistanceEvaluator _distanceEvaluator;
	private readonly ApproximationBuilder _approximationBuilder;

	public MainWindow(ExpressionParser expressionParser, VariationCalculator variationCalculator,
		DistanceEvaluator distanceEvaluator, ApproximationBuilder approximationBuilder)
	{
		_expressionParser = expressionParser;
		_variationCalculator = variationCalculator;
		_distanceEvaluator = distanceEvaluator;
		_approximationBuilder = approximationBuilder;

		InitializeComponent();

		SourceFunctionBlock.Changed += DrawSourceFunction;
	}

	private void UpdateApproximation(object? sender, RoutedEventArgs e)
	{
		try
		{
			var sourceFunction = SourceFunctionBlock.ParseFunction(_expressionParser);

			if (!sourceFunction.IsCorrect)
				return;

			var sourceVariation = _variationCalculator.GetVariation(sourceFunction);
			SourceFunctionBlock.SetVariation(sourceVariation);

			var approximationFunction = _approximationBuilder.BuildLinearApproximation(sourceFunction,
				sourceVariation * (double) VariationsRatio.Value!.Value);
			ApproximationFunctionBlock.FillParts(approximationFunction);
			ApproximationFunctionBlock.SetVariation(_variationCalculator.GetVariation(approximationFunction));
			ApproximationFunctionBlock.IsVisible = true;

			// ReSharper disable once SpecifyACultureInStringConversionExplicitly
			Distance.Text = _distanceEvaluator.GetDistance(sourceFunction, approximationFunction).ToString();
			DistanceBlock.IsVisible = true;

			UpdatePlot(sourceFunction, approximationFunction);
		}
		catch
		{
			// ignored
		}
	}

	private void DrawSourceFunction(object? sender, EventArgs e)
	{
		ApproximationFunctionBlock.IsVisible = false;

		try
		{
			var sourceFunction = SourceFunctionBlock.ParseFunction(_expressionParser);
			SourceFunctionBlock.SetVariation(_variationCalculator.GetVariation(sourceFunction));
			DistanceBlock.IsVisible = false;
			UpdatePlot(sourceFunction);
		}
		catch
		{
			SourceFunctionBlock.HideVariation();
			Plot.Plot.Clear();
			Plot.Refresh();
		}
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
}