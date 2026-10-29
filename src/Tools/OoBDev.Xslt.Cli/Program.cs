using global::System;

namespace OoBDev.Xslt.Cli;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0 || Array.Exists(args, a => a is "-h" or "--help" or "-?"))
        {
            Console.WriteLine(XsltCliOptions.Usage);
            return args.Length == 0 ? 1 : 0;
        }

        try
        {
            var options = XsltCliOptions.Parse(args);
            new SandboxedTransformer(options.Sandbox)
                .TransformAll(options.Template, options.Input, options.Exclude, options.InputType, options.Output, options.Merge);
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or global::System.IO.IOException or FormatException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
