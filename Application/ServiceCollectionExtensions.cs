using Application.ApproximationBuilders;
using Application.DistanceEvaluators;
using Application.ExtremeValues;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceCollectionExtensions
{
	public static void AddApplicationServices(this IServiceCollection collection)
	{
		collection.AddSingleton<IExtremeValuesEvaluator, ExtremeValuesEvaluator>();
		collection.AddSingleton<MonotonicityPartitioner>();
		collection.AddSingleton<VariationCalculator>();
		collection.AddSingleton<IDistanceEvaluator, UniformDistanceEvaluator>();
		collection.AddSingleton<IApproximationBuilder, ContinuousApproximationBuilder>();
		collection.AddSingleton<IApproximationBuilder, SingleDiscontinuityApproximationBuilder>();
		collection.AddSingleton<IApproximationBuilder, ApproximationBuilder>();
	}
}