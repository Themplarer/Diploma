using Application;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Domain;
using Intervals.Intervals;
using Intervals.Points;

namespace Gui.Controls;

internal sealed class FunctionPartDefinition : TemplatedControl
{
	private NumericUpDown _leftValue = null!, _rightValue = null!;
	private ToggleButton _isLeftValueIncluded = null!, _isRightValueIncluded = null!;
	private TextBox _function = null!;

	public static readonly StyledProperty<bool> IsReadOnlyProperty =
		AvaloniaProperty.Register<FunctionPartDefinition, bool>(nameof(IsReadOnly));

	public bool IsReadOnly
	{
		get => GetValue(IsReadOnlyProperty);
		set
		{
			SetValue(IsReadOnlyProperty, value);
			SetValue(CanBeToggledProperty, !value);
		}
	}

	public static readonly StyledProperty<bool> CanBeToggledProperty =
		AvaloniaProperty.Register<FunctionPartDefinition, bool>(nameof(CanBeToggled));

	private bool CanBeToggled
	{
		get => GetValue(CanBeToggledProperty);
		set => SetValue(CanBeToggledProperty, value);
	}

	public event EventHandler? Changed;

	public void Fill(FunctionPart functionPart)
	{
		EnsureChildrenAssigned();
		var (interval, (_, representation)) = functionPart;

		_leftValue.Value = interval.LeftValue;
		_rightValue.Value = interval.RightValue;
		_isLeftValueIncluded.IsChecked = interval.Left.Inclusion is Inclusion.Included;
		_isRightValueIncluded.IsChecked = interval.Right.Inclusion is Inclusion.Included;
		_function.Text = representation;
	}

	public FunctionPart GetDefinition(IExpressionParser expressionParser)
	{
		Point<decimal> CreatePoint(NumericUpDown numericUpDown, ToggleButton toggleButton) =>
			new(numericUpDown.Value!.Value, toggleButton.IsChecked!.Value ? Inclusion.Included : Inclusion.Excluded);

		Interval<decimal> GetInterval() =>
			new(CreatePoint(_leftValue, _isLeftValueIncluded), CreatePoint(_rightValue, _isRightValueIncluded));

		return new FunctionPart(GetInterval(), expressionParser.Parse(_function.Text!));
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);
		EnsureChildrenAssigned();
	}

	private void EnsureChildrenAssigned()
	{
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (_leftValue is not null)
			return;

		var children = this.GetTemplateChildren().ToArray();
		_leftValue = (NumericUpDown) children[2];
		_rightValue = (NumericUpDown) children[6];
		_isLeftValueIncluded = (ToggleButton) children[3];
		_isRightValueIncluded = (ToggleButton) children[5];
		_function = (TextBox) children[7];

		_leftValue.ValueChanged += (_, _) => OnChanged();
		_rightValue.ValueChanged += (_, _) => OnChanged();
		_isLeftValueIncluded.IsCheckedChanged += (_, _) => OnChanged();
		_isRightValueIncluded.IsCheckedChanged += (_, _) => OnChanged();
		_function.TextChanged += (_, _) => OnChanged();
	}

	private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}