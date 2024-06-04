using Application;
using Microsoft.Extensions.DependencyInjection;

namespace FunctionParser;

public static class ServiceCollectionExtensions
{
	public static void AddParser(this IServiceCollection collection) =>
		collection.AddSingleton<IExpressionParser, ExpressionParser>();
}