using Application;
using Domain;
using ILMath;

namespace FunctionParser;

internal sealed class ExpressionParser : IExpressionParser
{
	public RepresentableFunction Parse(string function) => new(ParseToFunction(function), function);

	private static Func<decimal, decimal> ParseToFunction(string function)
	{
		var evaluator = MathEvaluation.CompileExpression("src", function);
		return x =>
		{
			var evaluationContext = EvaluationContext.CreateDefault();
			evaluationContext.RegisterVariable(nameof(x), (double) x);
			return (decimal) evaluator.Invoke(evaluationContext);
		};
	}
}