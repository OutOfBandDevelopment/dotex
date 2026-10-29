# BinaryDataDecoders Numerics

**Date:** 2026-10-29

Migrated `BinaryDataDecoders.ToolKit/Numerics` into `OoBDev.System.Numerics`: `INumeric`, `FormattableNumber`, `ByteNumber`, `EngineeringNumber`, `ScientificNumber` and the three notation formatters, one type per file with XML docs. Documentation is in the Numerics section of [README.System.md](../../src/Framework/OoBDev.System/README.System.md).

**Changes from the source (breaking changes are accepted):**
- Parsing and formatting use the invariant culture (the source used the current culture, which broke `1.5kb` under a comma locale).
- Zero and non-finite values format unscaled (the source printed an exponent for 0).
- The SI symbol table is the internal `Symbols` class, no longer a public type with a public name that clashed with other namespaces; parsing no longer treats an unknown prefix as a number.
- A null value throws `ArgumentNullException`; an overflowing scaled value makes `TryParse` return false instead of throwing.
- Conversion operators call the constructor with a named argument, because the record copy constructor otherwise wins overload resolution and recurses forever.

**Verified:** 78 Unit tests in `OoBDev.System.Tests/Numerics` pass (the source's 7 test classes ported, plus zero, invalid input, null, unsupported type, conversions between notations and every implicit operator). Line coverage of the Numerics files is 58% to 100% per file before the last conversion tests; the whole `OoBDev.System` project coverage was not measured. Not run: the full solution test suite.
