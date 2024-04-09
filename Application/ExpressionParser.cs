using Domain;
using ILMath;

namespace Application;

public class ExpressionParser
{
	public RepresentableFunction Parse(string function) => new(ParseToFunction(function), function);

	public Func<double, double> ParseToFunction(string function)
	{
		var evaluator = MathEvaluation.CompileExpression("src", function);
		return x =>
		{
			var evaluationContext = EvaluationContext.CreateDefault();
			evaluationContext.RegisterVariable(nameof(x), x);
			return evaluator.Invoke(evaluationContext);
		};
	}
}