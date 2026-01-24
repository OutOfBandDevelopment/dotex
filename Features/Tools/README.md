# Tools - CLI Utilities Collection

**Status:** 📋 SPECIFICATION - Requirements Gathering
**Priority:** MIXED - See individual tool priorities
**Last Updated:** 2026-01-20

---

## Overview

The Incoming/Tools directory contains 4 standalone CLI applications for various development and hardware tasks. This document captures their features, use cases, and migration recommendations.

**Tools Found:**
1. OoBDev.De5000.Ble.Cli - Bluetooth LE device scanner (Hardware)

---

## Tool 1: OoBDev.De5000.Ble.Cli

### Overview

**Purpose:** Bluetooth Low Energy device scanner for discovering GATT services and characteristics on BLE hardware

**Target Hardware:** DE-5000 LCR meter with wireless serial bridge

**Status:** Incomplete/Early-stage

**Files:** 4 | **LOC:** 75 | **Framework:** net8.0-windows10.0.19041.0

### Use Cases

**UC-1: Hardware Device Discovery**
- **Actor:** Hardware Engineer / QA Tester
- **Goal:** Enumerate available Bluetooth LE devices
- **Flow:**
  1. Run CLI tool
  2. Tool scans for BLE devices
  3. Displays list of discovered devices with names and IDs
  4. Engineer identifies target device

**UC-2: GATT Service Inspection**
- **Actor:** Embedded Systems Developer
- **Goal:** Inspect GATT services, characteristics, and descriptors
- **Flow:**
  1. Run tool with device filter
  2. Tool connects to device
  3. Enumerates all services
  4. Lists all characteristics per service
  5. Displays descriptor information
  6. Developer documents protocol for integration

### Requirements

**FR-1:** Scan for Bluetooth LE devices within range
**FR-2:** Display device names, IDs, and signal strength
**FR-3:** Connect to specific device by ID
**FR-4:** Enumerate GATT services
**FR-5:** List characteristics for each service
**FR-6:** Display characteristic properties (read, write, notify)
**FR-7:** Read descriptor values

### Dependencies

- InTheHand.BluetoothLE v4.0.37
- Windows 10 SDK 19041+

### Migration Decision

**Priority:** LOW - Specialized hardware, incomplete implementation

**Options:**
1. **Complete & Migrate** - Finish implementation, add to ExternalServices/Hardware
2. **Archive** - Keep as reference for future BLE integration
3. **Delete** - Remove if no BLE hardware integration planned

**Recommendation:** Archive - Specialized use case, incomplete state

---

## Summary & Recommendations

| Tool | Status | Priority | Effort | Recommendation |
|------|--------|----------|--------|----------------|
| De5000.Ble.Cli | Incomplete | LOW | 40h (complete) or 2h (archive) | **Archive** - Specialized, incomplete |

**Recommended Actions:**
1. ✅ Archive De5000.Ble.Cli and ImageConverter.Cli

---

## Related Documentation

- [Incoming/Tools/](../../Incoming/Tools/) - Source code
- [TODO-migrations.md](../../TODO-migrations.md) - Migration tracking
- [Features/ContractParser/](../ContractParser/) - Related code generation feature

---

**Status:** Awaiting prioritization decision on BulkLlm consolidation
