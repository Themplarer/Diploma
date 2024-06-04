using Domain;

namespace Application.ExtremeValues;

internal sealed class ExtremeValuesEvaluator : IExtremeValuesEvaluator
{
	private readonly MonotonicityPartitioner _monotonicityPartitioner;

	public ExtremeValuesEvaluator(MonotonicityPartitioner monotonicityPartitioner) =>
		_monotonicityPartitioner = monotonicityPartitioner;

	public IEnumerable<decimal> GetLocalExtremes(PiecewiseFunction piecewiseFunction) =>
		_monotonicityPartitioner.BuildPartition(piecewiseFunction)
			.Select(i => i.LeftValue)
			.Append(piecewiseFunction.Range.RightValue);
}