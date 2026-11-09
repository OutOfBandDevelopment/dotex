using System.Collections.Generic;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>What <see cref="KsyImporter.Import"/> made: the frame, and what in the <c>.ksy</c> couldn't be carried over.</summary>
public sealed record KsyImportResult(FrameSchema? Schema, IReadOnlyList<string> Warnings);
