using System;
using System.Collections.Generic;

namespace OoBDev.DevTerm.Logging;

/// <summary>One record played back, with whatever the playback presenters rendered from it (only an <c>rx</c> record is ever rendered).</summary>
public sealed record PlaybackItem(int Index, SessionLogRecord Record, TimeSpan Offset, IReadOnlyList<PresenterOutput> Outputs);
