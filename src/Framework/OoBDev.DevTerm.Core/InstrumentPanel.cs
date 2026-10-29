using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>A control panel an <see cref="IInstrumentPanelProvider"/> opened for one chosen instrument.</summary>
public sealed record InstrumentPanel(UiDefinition Definition, IControlSurface Surface, string Title);
