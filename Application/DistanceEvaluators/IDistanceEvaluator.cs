using Domain;

namespace Application.DistanceEvaluators;

public interface IDistanceEvaluator
{
	decimal GetDistance(IFunction first, IFunction second);
}