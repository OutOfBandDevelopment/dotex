#nullable disable

using System.Reflection;

namespace OoBDev.Generations
{
    public class ProceduralGenerationDispatchProxy<T> : DispatchProxy
    {
        private IProcedualGenerationContext Context { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            //TODO: need to add the logic to support a simple backing store for results.

            var childContext = Context.Provider.CreateContext(targetMethod, args, default, default, default);
            var result = Context.Provider.Generate(childContext);
            return result;
        }

        public static T Create(IProcedualGenerationContext context)
        {
            object proxy = Create<T, ProceduralGenerationDispatchProxy<T>>();
            var unwrapped = (ProceduralGenerationDispatchProxy<T>)proxy;
            unwrapped.Context = context;
            var result = (T)proxy;
            return result;
        }
    }
#nullable enable
    public static class ProceduralGenerationDispatchProxy
    {
        public static T Create<T>(IProcedualGenerationContext context) =>
            ProceduralGenerationDispatchProxy<T>.Create(context);

        public static object? Create(IProcedualGenerationContext context) =>
            typeof(ProceduralGenerationDispatchProxy)
                .GetMethod(nameof(Create), genericParameterCount: 1, new[] { context.GetType() })
                .MakeGenericMethod(context.TargetType)
                .Invoke(null, new[] { context })
            ;
    }
}
