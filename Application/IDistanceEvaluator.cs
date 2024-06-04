using Domain;

namespace Application;

public interface IDistanceEvaluator
{
	decimal GetDistance(PiecewiseFunction first, PiecewiseFunction second);
}