using System.Collections;
using System.Linq;

namespace OoBDev.Generations.Attributes
{
    public class GenerateQueryableAttribute : GenerateCollectionAttribute
    {
        public static readonly new int DefaultPriority = GenerateCollectionAttribute.DefaultPriority - 1;
        public GenerateQueryableAttribute() => Priority = DefaultPriority;

        public override bool CanGenerateValue(IProcedualGenerationContext context) =>
            typeof(IQueryable).IsAssignableFrom(context.TargetType)
            ;

        public override object? GenerateValue(IProcedualGenerationContext context) =>
            ((IEnumerable?)base.GenerateValue(context))?.AsQueryable();
    }
}
