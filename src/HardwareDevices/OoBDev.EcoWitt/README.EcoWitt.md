# OoBDev.EcoWitt

Decoder for the HTTP push of an EcoWitt weather gateway (GW1000, GW1100, GW2000 and similar) configured with a "customized server" using the Ecowitt protocol.

## Description

Hardware driver, so it lives in `src/HardwareDevices`. The gateway posts an `application/x-www-form-urlencoded` body; this package turns it into a typed reading. It does no I/O: hosting the HTTP endpoint is the caller's job (a minimal API handler that passes the body to `EcoWittPushDecoder.Decode`).

| Type | Role |
|------|------|
| `EcoWittPushDecoder` | `Decode(body)`: splits the pairs, unescapes them and fills the reading |
| `EcoWittReading` | Typed values in the units the gateway sends (Fahrenheit, inches, mph, inHg) with Celsius, km/h, mm and hPa helpers; `null` for a field the push lacked |
| `ServiceCollectionExtensions` | `TryAddEcoWitt()` |

**Table 1 — Decoded fields**

| Field name in the push | Property |
|------------------------|----------|
| `stationtype`, `model`, `dateutc` | `StationType`, `Model`, `Timestamp` (UTC; `now` leaves it empty) |
| `tempinf`, `humidityin` | `IndoorTemperatureF`, `IndoorHumidity` |
| `tempf`, `humidity` | `OutdoorTemperatureF`, `OutdoorHumidity` |
| `baromrelin`, `baromabsin` | `RelativePressureInHg`, `AbsolutePressureInHg` |
| `winddir`, `windspeedmph`, `windgustmph` | `WindDirectionDegrees`, `WindSpeedMph`, `WindGustMph` |
| `rainratein`, `dailyrainin`, `weeklyrainin`, `monthlyrainin`, `yearlyrainin` | `RainRateInPerHour`, `DailyRainIn`, `WeeklyRainIn`, `MonthlyRainIn`, `YearlyRainIn` |
| `solarradiation`, `uv` | `SolarRadiationWm2`, `UvIndex` |

Any other field (extra sensors, battery flags) is kept by name in `Unrecognised`. A non-numeric value, a pair without `=` or a bad timestamp is reported in `Problems` and the rest of the push still decodes. `PASSKEY`, which identifies the gateway to the vendor cloud, is dropped and never stored.

## Not built

The gateway's local TCP API (binary, port 45000) is not decoded; it is tracked in `BACKLOG.md`. The tests use a representative push written from the protocol description, not a capture from a device; a real capture would confirm the unit assumptions for additional sensors.
