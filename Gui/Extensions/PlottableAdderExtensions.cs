using Application;
using Common;
using Domain;
using ScottPlot;

namespace Gui.Extensions;

internal static class PlottableAdderExtensions
{
	public static void ScatterPiecewiseFunction(this PlottableAdder plottableAdder, PiecewiseFunction piecewiseFunction,
		Color color, float lineWidth)
	{
		foreach (var (interval, (function, _)) in piecewiseFunction.Parts)
		{
			var xs = interval.Close().Split(Constants.PlotStepSize).ToArray();
			var ys = xs.Select(x => function((double) x)).ToArray();
			var scatter = xs.Length == 1 ? plottableAdder.ScatterPoints(xs, ys) : plottableAdder.ScatterLine(xs, ys);
			scatter.Color = color;
			scatter.LineWidth = lineWidth;
		}
	}
}