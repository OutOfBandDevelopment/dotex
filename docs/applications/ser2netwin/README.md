# ser2net-win documentation

`ser2net-win` bridges Windows COM ports to raw TCP or RFC 2217 and runs as a console app or a Windows Service. It hosts the Aeotec Z-Stick for the Z-Wave Hardware tests. The bridge code is the Framework library `OoBDev.System.IO.Ports.Bridge`; the command line is the application `OoBDev.Ser2NetWin.Cli` (assembly `ser2net-win`).

## Contents

- [Documents](#documents)
- [Code](#code)

## Documents

**Table 1 — Documents in this folder**

| Document | What it holds |
| --- | --- |
| [user-guide.md](user-guide.md) | Commands and options, generated from a run of the built tool |
| [architecture.md](architecture.md) | Parts, the one-client policy and how a client is handled |
| [usb-devices-and-containers.md](usb-devices-and-containers.md) | Selecting a port by USB id, the container recipe and the scope of the bridge |
| [testing.md](testing.md) | Test projects, tiers and the hardware test |
| [source/README.md](source/README.md) | The project guide as imported from the source repository (usage, settings, mDNS) |
| [bugs/](bugs/single-client-silent-reject.md) | Imported bug history: a second client was dropped without a reason (fixed in 0.0.4) |

## Code

**Table 2 — Where the code lives**

| Part | Location |
| --- | --- |
| Bridge library | `src/Framework/OoBDev.System.IO.Ports.Bridge` and `.Tests` |
| Command line | `src/Applications/Ser2NetWin/OoBDev.Ser2NetWin.Cli` and `.Tests` |
| Publish script | `scripts/ser2netwin/publish.ps1` |
