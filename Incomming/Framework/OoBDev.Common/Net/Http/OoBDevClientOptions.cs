using System.Collections.Generic;

namespace OoBDev.Common.Net.Http;

public class OoBDevClientOptions
{
    public const string OptionName = "OoBDevClients";

    public required Dictionary<string, string> ApiKeys { get; set; } = [];
}
