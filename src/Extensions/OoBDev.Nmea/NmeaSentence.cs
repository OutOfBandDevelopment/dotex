using System.Collections.Generic;

namespace OoBDev.Nmea;

/// <summary>
/// One NMEA 0183 sentence split into talker, type and fields. Typed views (<see cref="NmeaGga"/>, <see cref="NmeaRmc"/>,
/// <see cref="NmeaGsa"/>, <see cref="NmeaGsv"/>, <see cref="NmeaVtg"/>) read the fields of the sentence types they know.
/// </summary>
/// <param name="TalkerId">The first two characters, for example <c>GP</c>, <c>GN</c> or <c>GL</c>.</param>
/// <param name="SentenceType">The rest of the address field, for example <c>GGA</c>.</param>
/// <param name="Fields">The comma separated fields after the address, checksum removed; blank fields stay empty strings.</param>
/// <param name="Checksum">Whether the appended checksum, if any, matches.</param>
public sealed record NmeaSentence(string TalkerId, string SentenceType, IReadOnlyList<string> Fields, NmeaChecksum Checksum);
