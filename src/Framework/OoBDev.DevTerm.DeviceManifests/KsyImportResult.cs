using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>What <see cref="KsyImporter.Import"/> made: the frame, and what in the <c>.ksy</c> couldn't be carried over.</summary>
public sealed record KsyImportResult(FrameSchema? Schema, IReadOnlyList<string> Warnings);
