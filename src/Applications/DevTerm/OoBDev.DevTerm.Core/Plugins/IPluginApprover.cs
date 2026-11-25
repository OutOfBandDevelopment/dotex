namespace OoBDev.DevTerm;

/// <summary>Asks the user whether an out-of-process plugin may run. A front end supplies one; with none, only already-remembered plugins run.</summary>
public interface IPluginApprover
{
    PluginApprovalChoice Ask(PluginApprovalRequest request);
}
