# Ser2NetWin

Ser2NetWin bridges Windows COM ports to TCP (raw or RFC 2217, optionally advertised over mDNS). It hosts the Aeotec Z-Stick for the Z-Wave Hardware tests and is a Windows-native stand-in for Linux `ser2net`.

**Status:** migrated (2026-11-05). The bridge core is in the Framework (`OoBDev.System.IO.Ports.Bridge`), the command line is `src/Applications/Ser2NetWin`.

**Table 1 — Documents**

| Document | Holds |
|----------|-------|
| [migration-plan.md](migration-plan.md) | project map, layer decision (option A) and the steps taken |
| [Application docs](../../applications/ser2netwin/README.md) | architecture, testing, CLI guide and imported history |
