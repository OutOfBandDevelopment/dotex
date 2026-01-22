namespace OoBDev.Generations
{
    public interface IProceduralGenerationTypeBuilderFactory
    {
        object? Create(IProcedualGenerationContext context);
    }
}
