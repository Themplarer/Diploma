using Domain;

namespace Application;

public interface IExpressionParser
{
	RepresentableFunction Parse(string function);
}