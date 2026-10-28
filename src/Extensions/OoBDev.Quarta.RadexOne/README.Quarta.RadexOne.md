# OoBDev.Quarta.RadexOne

Quarta Radex One dosimeter: prefixed frames with a length field and outer and inner checksums.

## Description

**Status: stub.** The project builds and holds the provider key only; nothing is decoded yet. Source: dev-term `Devices.RadexOne` with merges M3 and M4 as documented hypotheses. Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md), plan: [device-decoding-migration-plan.md](../../../docs/migration/device-decoding-migration-plan.md).

Implements `IDeviceDefinitionReceiver<T>` and `IDeviceDefinitionTransmitter<T>` from `OoBDev.System.Abstractions`; bytes arrive through the existing adapters, so the package never opens a port.

## Not yet verified on hardware

Everything: no behaviour is ported yet.