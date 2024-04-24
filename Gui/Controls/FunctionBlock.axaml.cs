using Application;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Domain;

namespace Gui.Controls;

public class FunctionBlock : TemplatedControl
{
	private WrapPanel _variationBlock = null!;
	private TextBlock _variationLabel = null!;
	private StackPanel _parts = null!;

	public event EventHandler? Changed;

	public static readonly StyledProperty<string> LabelProperty =
		AvaloniaProperty.Register<FunctionBlock, string>(nameof(Label));

	public string Label
	{
		get => GetValue(LabelProperty);
		set => SetValue(LabelProperty, value);
	}

	public static readonly StyledProperty<bool> IsReadOnlyProperty =
		AvaloniaProperty.Register<FunctionBlock, bool>(nameof(IsReadOnly));

	public bool IsReadOnly
	{
		get => GetValue(IsReadOnlyProperty);
		set => SetValue(IsReadOnlyProperty, value);
	}

	public static readonly StyledProperty<bool> IsVisibleByDefaultProperty =
		AvaloniaProperty.Register<FunctionBlock, bool>(nameof(IsVisibleByDefault), true);

	public bool IsVisibleByDefault
	{
		get => GetValue(IsVisibleByDefaultProperty);
		set => SetValue(IsVisibleByDefaultProperty, value);
	}

	public PiecewiseFunction ParseFunction(ExpressionParser expressionParser) =>
		new(_parts.Children
			.SkipLast(1)
			.Cast<FunctionPartDefinition>()
			.Select(f => f.GetDefinition(expressionParser))
			.ToArray());

	public void SetVariation(double variation)
	{
		_variationBlock.IsVisible = true;
		// ReSharper disable once SpecifyACultureInStringConversionExplicitly
		_variationLabel.Text = variation.ToString();
	}

	public void HideVariation() => _variationBlock.IsVisible = false;

	public void FillParts(PiecewiseFunction piecewiseFunction)
	{
		_parts.Children.Clear();

		foreach (var functionPart in piecewiseFunction.Parts)
			AppendPart(functionPart);
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);
		var children = this.GetTemplateChildren().ToArray();
		_variationBlock = (WrapPanel) children[3];
		_variationLabel = (TextBlock) children[5];
		_parts = (StackPanel) children[8];

		if (!IsReadOnly)
			AppendPart();

		IsVisible = IsVisibleByDefault;
	}

	private void AppendPart(FunctionPart? functionPart = null)
	{
		var functionPartDefinition = new FunctionPartDefinition
		{
			IsReadOnly = IsReadOnly
		};

		functionPartDefinition.Loaded += (_, _) =>
		{
			if (functionPart is not null)
				functionPartDefinition.Fill(functionPart.Value);
		};

		if (!IsReadOnly)
		{
			functionPartDefinition.Opacity = 0.5;
			functionPartDefinition.Changed += (_, _) => functionPartDefinition.Opacity = 1;
			functionPartDefinition.Changed += (sender, _) =>
			{
				if (_parts.Children.LastOrDefault() is FunctionPartDefinition lastDefinition && lastDefinition.Equals(sender))
					AppendPart();
			};
			functionPartDefinition.Changed += (_, _) => OnChanged();
		}

		_parts.Children.Add(functionPartDefinition);
	}

	protected virtual void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}