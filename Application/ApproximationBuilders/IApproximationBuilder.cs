using Domain;

namespace Application.ApproximationBuilders;

public interface IApproximationBuilder
{
	bool IsFallback { get; }

	PiecewiseFunction? Build(PiecewiseFunction sourceFunction, decimal variation, decimal newVariation);
}