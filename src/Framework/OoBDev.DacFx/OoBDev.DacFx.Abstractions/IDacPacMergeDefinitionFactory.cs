namespace OoBDev.DacFx;

public interface IDacPacMergeDefinitionFactory
{
    IDacPacMergeDefinition Create(IDacPacMergeTemplate template);
}