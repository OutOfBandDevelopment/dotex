# SCPI definite-length block replies (2026-10-29)

`ScpiReplySegmenter` now cuts an IEEE 488.2 definite-length block (`#<n><length>` followed by that many bytes, for example a screen dump) as one segment, so CR, LF or `#` bytes inside the data no longer split it. `ScpiMessageDecoder` returns the data without its prefix in the new `ScpiReply.Block`; text replies are unchanged (`Block` is empty).

A `#` that is not followed by a digit 1 to 9 and the right number of digits stays an ordinary line. A block that is shorter than its declared length waits for more bytes. Indefinite blocks (`#0`) and block reassembly for profiles with an empty terminator are not supported; both are noted in `README.Scpi.md`.

Tests (Unit, 35 pass in `OoBDev.Scpi.Tests`): a block with CR/LF/`#` inside, split across two arrivals and followed by a text line; four non-block `#` lines; a truncated block decoded as text. Not run against a real instrument.
