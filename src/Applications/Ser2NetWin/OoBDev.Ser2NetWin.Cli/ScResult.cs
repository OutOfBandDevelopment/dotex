using OoBDev.System.IO.Ports.Bridge;
namespace OoBDev.Ser2NetWin.Cli;

public sealed record ScResult(int ExitCode, string Output)
{
    public bool Succeeded => ExitCode == 0;
}
