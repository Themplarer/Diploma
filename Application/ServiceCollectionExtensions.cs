using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceCollectionExtensions
{
	public static void AddApplicationServices(this IServiceCollection collection)
	{
		collection.AddSingleton<ExpressionParser>();
		collection.AddSingleton<VariationCalculator>();
		collection.AddSingleton<DistanceEvaluator>();
		collection.AddSingleton<ApproximationBuilder>();
	}
}