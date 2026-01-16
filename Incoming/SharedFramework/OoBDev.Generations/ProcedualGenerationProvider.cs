using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace OoBDev.Generations
{
    public class ProcedualGenerationProvider : IProcedualGenerationProvider
    {
        public IProcedualGenerationContextBuilder ContextBuilder { get; }
        public IProceduralGenerationDispatchProxyFactory DispatchFactory { get; }
        public IServiceProvider? ServiceProvider { get; }
        public IProceduralGenerationTypeBuilderFactory TypeBuilder { get; }

        public ProcedualGenerationProvider(
            IServiceProvider? serviceProvider = null,
            IProcedualGenerationContextBuilder? contextBuilder = null,
            IProceduralGenerationDispatchProxyFactory? dispatchFactory = null,
            IProceduralGenerationTypeBuilderFactory? typeBuilder = null
            )
        {
            ServiceProvider = serviceProvider;
            ContextBuilder = contextBuilder ??
                ((IProcedualGenerationContextBuilder?)ServiceProvider?.GetService(typeof(IProcedualGenerationContextBuilder))) ??
                new ProcedualGenerationContextBuilder()
                ;
            DispatchFactory = dispatchFactory ??
                ((IProceduralGenerationDispatchProxyFactory?)ServiceProvider?.GetService(typeof(IProceduralGenerationDispatchProxyFactory))) ??
                new ProceduralGenerationDispatchProxyFactory()
                ;
            TypeBuilder = typeBuilder ??
                ((IProceduralGenerationTypeBuilderFactory?)ServiceProvider?.GetService(typeof(IProceduralGenerationTypeBuilderFactory))) ??
                new ProceduralGenerationTypeBuilderFactory()
                ;
        }

        static ProcedualGenerationProvider() => ProcedualGenerationRegister.EnsureExtension();

        public object? Generate(IProcedualGenerationContext context, bool required = true) =>
            Logger(context, () =>
                 GetGenerators(context, context.Attributes).FirstOrDefault() ??
                 GetGenerators(context, TypeDescriptor.GetAttributes(context.TargetType).OfType<Attribute>()).FirstOrDefault() ??
                 GetGenerators(context, TypeDescriptor.GetAttributes(typeof(object)).OfType<Attribute>()).FirstOrDefault() ??
                 required switch
                 {
                     true => throw new NotSupportedException($@"""{context.TargetType}"" is not supported"),
                     false => null
                 }
            )?.GenerateValue(context);

        internal IEnumerable<IGenerateObject> GetGenerators(IProcedualGenerationContext context, IEnumerable<Attribute> attributes)
            => from g in attributes.OfType<IGenerateObject>()
               orderby g.Priority
               where g.CanGenerateValue(context)
               select g;

        internal IGenerateObject? Logger(IProcedualGenerationContext context, Func<IGenerateObject?> generator)
        {
            try
            {
                var generate = generator();
                if (generate != null)
                {
                    Trace.WriteLine($"Generate \"{context.TargetType}\" based on {context.Seed} by \"{generate?.GetType()}\"", "ProcedualGeneration-Provider");
                }
                else
                {
                    Trace.WriteLine($"Nothing generated for \"{context.TargetType}\" based on {context.Seed}", "ProcedualGeneration-Provider");
                }
                return generate;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to Generate \"{context.TargetType}\" based on {context.Seed} by \"{ex.Message}\"", "ProcedualGeneration-Provider-Error");
                Trace.WriteLine(ex, "ProcedualGeneration-Provider-Error-Details");
                throw;
            }
        }

        public IProcedualGenerationContext CreateContext(Type type, IEnumerable<Attribute>? attributes, IProcedualGenerationContext? context, int? index) =>
            ContextBuilder.CreateContext(this, context, index ?? 0, type, attributes ?? Enumerable.Empty<Attribute>());

        public IProcedualGenerationContext CreateContext(MethodBase method, object[]? args, IEnumerable<Attribute>? attributes, IProcedualGenerationContext? context, int? index) =>
            ContextBuilder.CreateContext(this, context, index ?? 0, method, args ?? Array.Empty<object>(), attributes ?? Enumerable.Empty<Attribute>());
    }
}
