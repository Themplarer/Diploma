using Domain;
using Intervals.Intervals;

namespace Application.ApproximationBuilders;

internal sealed class ContinuousApproximationBuilder : IApproximationBuilder
{
	private readonly MonotonicityPartitioner _monotonicityPartitioner;

	public ContinuousApproximationBuilder(MonotonicityPartitioner monotonicityPartitioner) =>
		_monotonicityPartitioner = monotonicityPartitioner;

	public bool IsFallback => false;

	public PiecewiseFunction? Build(PiecewiseFunction sourceFunction, decimal variation, decimal newVariation)
	{
		if (!sourceFunction.IsContinuous())
			return null;

		var partition = _monotonicityPartitioner.BuildPartition(sourceFunction).ToArray();
		var rho = (variation - newVariation) / (2 * partition.Length);
		var partitionParams = partition
			.Select(i =>
			{
				var leftValue = sourceFunction.Evaluate(i.LeftValue)!.Value;
				var rightValue = sourceFunction.Evaluate(i.RightValue)!.Value;
				var partVariation = Math.Abs(leftValue - rightValue);
				var newPartVariation = partVariation - 2 * rho;
				return (Interval: i, Scale: newPartVariation / partVariation,
					Offset: (leftValue + rightValue) * rho / partVariation);
			})
			.ToArray();

		return new PiecewiseFunction(sourceFunction.Parts
			.SelectMany(_ => partitionParams,
				(p, t) => (Interval: p.Interval.Overlap(t.Interval).SingleOrDefault(), p.Function, t.Scale, t.Offset))
			.Where(t => t.Interval is not null)
			.Select(t => new FunctionPart(t.Interval!,
				new RepresentableFunction(x => t.Scale * t.Function.Method(x) + t.Offset,
					$"{t.Scale} * ({t.Function.Representation}) + {t.Offset}")))
			.ToArray());
	}
}