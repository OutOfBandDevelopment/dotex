# OoBDev.EventSourcing.SqlServer.Database

SQL project that builds the dacpac for the SQL Server event and entity stores (`OoBDev.EventSourcing.SqlServer`; design in `docs/design/CqrsEventSourcing/`), in the style of `OoBDev.Example.DB`. Tables: `dbo.Events` (identity `Position`, unique `(StreamId, Sequence)`) and `dbo.Entities` (key `(Type, Key)`). Deploy with `SqlPackage` or `dotnet publish`. The `OoBDev.EventSourcing.EntityFrameworkCore.Tests` project compares the built dacpac with the EF model so the two cannot drift.
