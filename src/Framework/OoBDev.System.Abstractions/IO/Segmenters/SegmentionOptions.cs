﻿using System;

namespace OoBDev.System.IO.Segmenters;

[Flags]
public enum SegmentionOptions
{
    None = 0,
    SkipInvalidSegment = 1,
    SecondStartInvalid = 2,
}