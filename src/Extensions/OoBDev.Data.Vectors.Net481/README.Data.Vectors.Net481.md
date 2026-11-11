# OoBDev.Data.Vectors.Net481

The SQL CLR build of `OoBDev.Data.Vectors`: the same source compiled for the framework SQL Server loads, producing the assembly (`OoBDev.Data.Vectors.dll`, x64) and the dacpac that carries the `[embedding]` types and functions.

## How it is built

- The project has no code of its own: it compiles every `.cs` file of `OoBDev.Data.Vectors` (`Compile Include="..\OoBDev.Data.Vectors\**\*.cs"`), so a change there is a change here. Nullable and implicit usings are off because the sources are shared with the .NET Framework build.
- On Windows it targets `net48`; elsewhere it targets `netstandard2.1` so the solution still builds on Linux. SQL Server itself only loads the .NET Framework assembly.
- `AssemblyName` is `OoBDev.Data.Vectors`, so the assembly deployed to SQL Server has the same name as the library.

## Where it is used

Do not reference it from application code. `OoBDev.Data.Vectors.DB` and `OoBDev.Example.DB` reference its dacpac (target `GenerateDacPac`), and `OoBDev.DacPacCompiler.Cli` reads the assembly to create that dacpac. The deployment steps and SQL Server prerequisites (CLR enabled, assembly trusted) are in the [Data.Vectors.DB readme](../OoBDev.Data.Vectors.DB/README.Data.Vectors.DB.md). The type and function reference is the [Data.Vectors readme](../OoBDev.Data.Vectors/README.Data.Vectors.md).
