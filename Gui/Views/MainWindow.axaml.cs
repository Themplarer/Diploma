using Application;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Domain;
using Gui.Controls;

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

	private void AppendPart(Panel functionDefinitions, EventHandler<KeyEventArgs> updateAction)
	{
		var functionDefinition = new FunctionPartDefinition();
		functionDefinition.KeyDown += updateAction;
		functionDefinitions.Children.Add(functionDefinition);
	}

	private void Update(object? sender, RoutedEventArgs e)
	{
		var sourceFunction = ParseFunction(SourceFunctionDefinitions);
		var sourceVariation = GetVariation(sourceFunction);
		var approximationFunction = _approximationBuilder.BuildLinearApproximation(sourceFunction, sourceVariation / 4);

		SourceVariation.Text = sourceVariation.ToString();
		ApproximationVariation.Text = GetVariation(approximationFunction).ToString();
		Distance.Text = _distanceEvaluator.GetDistance(sourceFunction, approximationFunction).ToString();

		if (!sourceFunction.IsCorrect || !approximationFunction.IsCorrect ||
		    sourceFunction.Range != approximationFunction.Range)
			return;

		Draw(sourceFunction, approximationFunction);
	}

	private double GetVariation(PiecewiseFunction function) => _variationCalculator.GetVariation(function);

	private void Draw(PiecewiseFunction sourceFunction, PiecewiseFunction approximationFunction)
	{
		Plot.Plot.Clear();
		Plot.Plot.Add.Function(sourceFunction.Evaluate);
		Plot.Plot.Add.Function(approximationFunction.Evaluate);

		var (left, right, _) = sourceFunction.Range;
		Plot.Plot.Axes.SetLimits((double) left, (double) right);
		Plot.Plot.Axes.AutoScale();
		Plot.Refresh();
	}

	private PiecewiseFunction ParseFunction(Panel functionDefinitions) =>
		new(functionDefinitions.Children
			.SkipLast(1)
			.Cast<FunctionPartDefinition>()
			.Select(f => f.GetDefinition(_expressionParser))
			.ToArray());
}