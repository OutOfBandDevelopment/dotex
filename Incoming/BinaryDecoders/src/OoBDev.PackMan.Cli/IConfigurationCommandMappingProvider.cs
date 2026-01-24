using System.Collections.Generic;

namespace OoBDev.PackMan.Cli;

public interface IConfigurationCommandMappingProvider
{
    IDictionary<string, string> SwitchMappings { get; }
}
