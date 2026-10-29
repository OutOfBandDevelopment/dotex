using System;
using System.IO;
using OoBDev.DevTerm.Schemas;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: OoBDev.DevTerm.Schemas <output-folder>");
    return 1;
}

Directory.CreateDirectory(args[0]);
foreach (var (name, text) in SchemaGenerator.Generate())
{
    File.WriteAllText(Path.Combine(args[0], name), text);
    Console.WriteLine($"wrote {name}");
}

return 0;
