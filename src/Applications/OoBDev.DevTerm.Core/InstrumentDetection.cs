using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>What an auto-detect settled on: the <paramref name="Choice"/> to open (an instrument name, or the generic one) and a one-line <paramref name="Message"/> for the output pane.</summary>
public sealed record InstrumentDetection(string Choice, string Message);
