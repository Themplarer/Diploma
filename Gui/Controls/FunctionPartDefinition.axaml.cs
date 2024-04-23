using Application;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Domain;
using Intervals.Intervals;
using Intervals.Points;

namespace Gui.Controls;

public class FunctionPartDefinition : TemplatedControl
{
	private NumericUpDown _leftValue = null!, _rightValue = null!;
	private ToggleButton _isLeftValueIncluded = null!, _isRightValueIncluded = null!;
	private TextBox _function = null!;

	public FunctionPart GetDefinition(ExpressionParser expressionParser) =>
		new(GetInterval(), expressionParser.Parse(_function.Text!));

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);
		var children = this.GetTemplateChildren().ToArray();
		_leftValue = (NumericUpDown) children[2];
		_rightValue = (NumericUpDown) children[6];
		_isLeftValueIncluded = (ToggleButton) children[3];
		_isRightValueIncluded = (ToggleButton) children[5];
		_function = (TextBox) children[7];
	}

	private Interval<decimal> GetInterval()
	{
		Point<decimal> CreatePoint(NumericUpDown numericUpDown, ToggleButton toggleButton) =>
			new(numericUpDown.Value!.Value, toggleButton.IsChecked!.Value ? Inclusion.Included : Inclusion.Excluded);

		return new Interval<decimal>(CreatePoint(_leftValue, _isLeftValueIncluded),
			CreatePoint(_rightValue, _isRightValueIncluded));
	}
}