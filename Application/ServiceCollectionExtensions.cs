using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceCollectionExtensions
{
	public static void AddApplicationServices(this IServiceCollection collection)
	{
		collection.AddSingleton<VariationCalculator>();
		collection.AddSingleton<IDistanceEvaluator, UniformDistanceEvaluator>();
		collection.AddSingleton<ApproximationBuilder>();
	}
}