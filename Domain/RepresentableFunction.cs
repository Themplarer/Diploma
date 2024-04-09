namespace Domain;

public readonly record struct RepresentableFunction(Func<double, double> Method, string Representation);