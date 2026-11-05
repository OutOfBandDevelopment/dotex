using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>The main window's position and size when a project was saved (device-independent pixels), and whether it was maximized. Only the WPF front end uses it.</summary>
public sealed record ProjectWindowBounds(double Left, double Top, double Width, double Height, bool Maximized);
