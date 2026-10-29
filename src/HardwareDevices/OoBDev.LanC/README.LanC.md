# OoBDev.LanC

Sony LANC (Logic Application Control) command and status tables for camcorders and VTRs, as byte enums. LANC is a one-wire serial control bus; the package holds the values only.

## Description

Source: BinaryDataDecoders `LanC`; tables follow <http://www.boehmel.de/lanc.htm#byte0>.

| Type | Role |
|------|------|
| `LanCSubCommand` | First byte: which command table the second byte belongs to |
| `LanCCameraVtrCommand` | Second byte for the camera and VTR normal table (transport, programs, menus) |
| `LanCCameraSpecialCommand` | Second byte for the camera special table (zoom speeds, focus, iris, record) |
| `LanCStatusCode` | Status byte reported by the device |

Names that were typos or held underscores in the source were renamed (for example `SillNormal` is `StillNormal`, `X1_10` is `X1Of10`); byte values are unchanged. Gaps in the tables (`// ? = 0x24`) are unknown codes and stay unnamed.

## Verified

Unit tests check that every table has unique byte values and spot-check keys against the published table. Nothing was run against a camera.

## Not yet verified on hardware

No Hardware test: no LANC interface or camera is attached, and the package has no transport or frame builder yet.
