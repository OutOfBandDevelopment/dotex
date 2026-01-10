namespace OoBDev.MigrationHelper.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = @"C:\repo\merge-em\dotex\Incomming\Framework";
            var sourcePrefix = "ERisk";
            var targetPrefix = "OoBDev";

            foreach (var folder in Directory.EnumerateDirectories(path, "*.*", SearchOption.AllDirectories))
            {
                var dir = Path.GetDirectoryName(folder);
                var current = Path.GetFileName(folder);
                if (current.StartsWith(sourcePrefix))
                {
                    var next = Path.Combine(dir, current.Replace(sourcePrefix, targetPrefix));
                    Console.WriteLine($"{current}");
                    Directory.Move(folder, next);
                }
            }

            foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                var dir = Path.GetDirectoryName(file);
                var current = Path.GetFileName(file);
                if (current.StartsWith(sourcePrefix))
                {
                    var next = Path.Combine(dir, current.Replace(sourcePrefix, targetPrefix));
                    Console.WriteLine($"{current}");
                    File.Move(file, next);
                }
            }

            foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    var content = File.ReadAllText(file);

                    if (!content.Contains(sourcePrefix)) continue;

                    content = content.Replace(sourcePrefix, targetPrefix);

                    if (content.Contains("\0"))
                    {
                        Console.WriteLine($"Skip: {file}");
                    }

                    Console.WriteLine($"{file}");
                    File.WriteAllText(file, content);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"{ex.Message}");
                }
            }
        }
    }
}
