namespace Domain;

public readonly record struct RepresentableFunction(Func<decimal, decimal> Method, string Representation);