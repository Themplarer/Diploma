﻿using Intervals.Intervals;

namespace Domain;

public readonly record struct FunctionPart(Interval<decimal> Interval, RepresentableFunction Function);