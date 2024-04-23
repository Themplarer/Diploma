using Application;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Common;
using Domain;
using Gui.Controls;
using ScottPlot;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

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
		AppendPart(SourceFunctionDefinitions, UpdateSourceDefinitions);
		AppendPart(ApproximationFunctionDefinitions, UpdateApproximationDefinitions);
	}

	private void UpdateSourceDefinitions(object? sender, KeyEventArgs e) =>
		UpdateDefinitions(sender, SourceFunctionDefinitions, UpdateSourceDefinitions);

	private void UpdateApproximationDefinitions(object? sender, KeyEventArgs e) =>
		UpdateDefinitions(sender, ApproximationFunctionDefinitions, UpdateApproximationDefinitions);

	private void UpdateDefinitions(object? sender, Panel functionDefinitions, EventHandler<KeyEventArgs> updateAction)
	{
		if (functionDefinitions.Children.LastOrDefault() is { } lastDefinition && lastDefinition.Equals(sender))
			AppendPart(functionDefinitions, updateAction);
	}

	private void AppendPart(Panel functionDefinitions, EventHandler<KeyEventArgs>? updateAction = null)
	{
		var functionDefinition = new FunctionPartDefinition();
		functionDefinition.KeyDown += updateAction;
		functionDefinitions.Children.Add(functionDefinition);
	}

	private void UpdateApproximation(object? sender, RoutedEventArgs e)
	{
		var sourceFunction = ParseFunction(SourceFunctionDefinitions);
		var sourceVariation = _variationCalculator.GetVariation(sourceFunction);
		var approximationFunction = _approximationBuilder.BuildLinearApproximation(sourceFunction, sourceVariation / 4);

		SourceVariation.Text = sourceVariation.ToString();
		ApproximationVariation.Text = _variationCalculator.GetVariation(approximationFunction).ToString();
		Distance.Text = _distanceEvaluator.GetDistance(sourceFunction, approximationFunction).ToString();

		if (!sourceFunction.IsCorrect || !approximationFunction.IsCorrect ||
		    sourceFunction.Range != approximationFunction.Range)
			return;

		UpdatePlot(sourceFunction, approximationFunction);
	}

	private void UpdatePlot(PiecewiseFunction sourceFunction, PiecewiseFunction approximationFunction)
	{
		Plot.Plot.Clear();

		var (left, right, _) = sourceFunction.Range;
		Plot.Plot.Axes.SquareUnits();
		Plot.Plot.Axes.SetLimits((double) left, (double) right);

		DrawFunction(sourceFunction, Colors.DarkCyan, 2);
		DrawFunction(approximationFunction, Colors.Red, 1);

		Plot.Refresh();
	}

	private void DrawFunction(PiecewiseFunction sourceFunction, Color color, float lineWidth)
	{
		foreach (var (interval, (function, _)) in sourceFunction.Parts)
		{
			var xs = interval.Close().Split(Constants.PlotStepSize).ToArray();
			var ys = xs.Select(x => function((double) x)).ToArray();
			var scatter = xs.Length == 1 ? Plot.Plot.Add.ScatterPoints(xs, ys) : Plot.Plot.Add.ScatterLine(xs, ys);
			scatter.Color = color;
			scatter.LineWidth = lineWidth;
		}
	}

	private PiecewiseFunction ParseFunction(Panel functionDefinitions) =>
		new(functionDefinitions.Children
			.SkipLast(1)
			.Cast<FunctionPartDefinition>()
			.Select(f => f.GetDefinition(_expressionParser))
			.ToArray());
}