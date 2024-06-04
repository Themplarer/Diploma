using Application.ExtremeValues;
using Common;
using Domain;

namespace Application;

public class VariationCalculator
{
	private readonly IExtremeValuesEvaluator _extremeValuesEvaluator;

	public VariationCalculator(IExtremeValuesEvaluator extremeValuesEvaluator) =>
		_extremeValuesEvaluator = extremeValuesEvaluator;

	public decimal GetVariation(FunctionPart functionPart) => GetVariation(functionPart.WrapToFunction());

	public decimal GetVariation(PiecewiseFunction piecewiseFunction) =>
		piecewiseFunction.Parts
			.SelectMany(p => _extremeValuesEvaluator.GetLocalExtremes(p.WrapToFunction()), (f, x) => f.Function.Method(x))
			.GetBigrams()
			.Sum(t => Math.Abs(t.First - t.Second));
}