using System.Collections.Generic;

namespace OoBDev.DevTerm;

/// <summary>An out-of-process plugin's program: <c>{ "command": "python", "arguments": ["{folder}/shout.py"], "replyTimeoutMs": 2000 }</c>. <c>{folder}</c> in either is replaced by the plugin's folder.</summary>
public sealed class PluginProcess
{
    public string Command { get; set; } = string.Empty;

    public IList<string> Arguments { get; set; } = [];

    public int ReplyTimeoutMs { get; set; } = 2000;
}
