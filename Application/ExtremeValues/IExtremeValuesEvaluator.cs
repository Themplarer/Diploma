using Domain;

namespace Application.ExtremeValues;

public interface IExtremeValuesEvaluator
{
	IEnumerable<decimal> GetLocalExtremes(PiecewiseFunction piecewiseFunction);
}