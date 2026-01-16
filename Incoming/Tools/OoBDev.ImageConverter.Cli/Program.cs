using ImageMagick;

namespace OoBDev.ImageConverter.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = @"C:\Images\Too Close";

            string[] formats = [".heic", ".nef"];
            var files = formats.SelectMany(f => Directory.EnumerateFiles(path, $"*{f}", SearchOption.AllDirectories));

            async Task process(string file)
            {
                var outFile = Path.ChangeExtension(file, ".jpg");

                if (File.Exists(outFile)) return;

                Console.WriteLine(file.Replace(path, "."));
                using var image = new MagickImage(file);
                image.Format = MagickFormat.Jpeg;
                image.Quality = 85;
                await image.WriteAsync(outFile);
            }

            var tasks = files.Select(f=>process(f)).ToArray();

            Task.WaitAll(tasks);
        }
    }
}
