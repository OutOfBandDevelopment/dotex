using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace OoBDev.Generations
{
    //TODO: this is a work in progress... abstract classes are not supported at this time
    public class ProceduralGenerationTypeBuilderFactory : IProceduralGenerationTypeBuilderFactory
    {
        public object? Create(IProcedualGenerationContext context)
        {
            if (!context.TargetType.IsAbstract)
                throw new NotSupportedException($"{context.TargetType} is not supported only abstract classes may be dispatched");

            //var targetType = GetType(context);

            throw new NotImplementedException();
        }

        private static readonly ConcurrentDictionary<Type, Type> _types = new ConcurrentDictionary<Type, Type>();

        //internal static object? Example(IProcedualGenerationContext context)
        //{
        //    //__arglist
        //}

        internal static Type GetType(IProcedualGenerationContext context)
        {
            return _types.GetOrAdd(context.TargetType, t => BuildType(context, t));

            static Type BuildType(IProcedualGenerationContext context, Type abstraction)
            {
                var moduleBuilder = GetModuleBuilder();
                var typeBuilder = moduleBuilder.DefineType($"Generated_{context.TargetType.AssemblyQualifiedName}");
                typeBuilder.SetParent(context.TargetType);

                var _context = typeBuilder.DefineField("_context", typeof(IProcedualGenerationContext), FieldAttributes.Private);
                var _ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(IProcedualGenerationContext) });



                //TODO: add constructor and a backing field for the context

                var abstractMethods = abstraction.GetMethods().Where(m => m.IsAbstract).ToArray();
                foreach (var abstractMethod in abstractMethods.Where(m => m.IsSpecialName))
                {
                    var methodBuilder = typeBuilder.DefineMethod(
                        abstractMethod.Name,
                        abstractMethod.Attributes,
                        abstractMethod.ReturnType,
                        abstractMethod.GetParameters().Select(p => p.ParameterType).ToArray()
                        );
                }

                //    .Concat(typeBuilder.GetMethods(BindingFlags.NonPublic))
                //    .Where(m => m.IsAbstract)
                //    .ToArray()
                //    ;

                //  abstraction.GetMethods(BindingFlags.)


                //typeBuilder.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.vis)

                /*
Method 'get_Abstract' in type 'Generated_OoBDev.Generations.Tests.TestTargets.IncompleteClass, OoBDev.Generations.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' from assembly '696b7702-300e-4f2a-ae46-02a3a33100ff, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' does not have an implementation.
                */

                //    var typeBuilder = GetModuleBuilder()
                //        .DefineType(
                //            $"Generated_{context.TargetType.AssemblyQualifiedName}",
                //            TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                //            abstraction)
                //        ;


                var type = typeBuilder.CreateType();
                return type;
            }
        }

        private static ModuleBuilder? _md;
        internal static ModuleBuilder GetModuleBuilder()
        {
            return _md ??= MakeModuleBuilder();

            static ModuleBuilder MakeModuleBuilder()
            {
                var assemblyName = new AssemblyName(Guid.NewGuid().ToString());
                var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var module = assembly.DefineDynamicModule(Guid.NewGuid().ToString());
                return module;
            }
        }

    }
}
/*
 .class public auto ansi beforefieldinit OoBDev.Generations.Tests.TestTargets.ExampleCompleted
    extends OoBDev.Generations.Tests.TestTargets.IncompleteClassBase
{
    .field private initonly class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext _context

    .property instance int32 Abstract()
    {
        .get instance int32 OoBDev.Generations.Tests.TestTargets.ExampleCompleted::get_Abstract()
        {
            .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = (
                01 00 00 00
            )
            IL_0000: ldarg.0
            IL_0001: ldfld int32 OoBDev.Generations.Tests.TestTargets.ExampleCompleted::'<Abstract>k__BackingField'
            IL_0006: ret
        }
        .set instance void OoBDev.Generations.Tests.TestTargets.ExampleCompleted::set_Abstract(int32)
        {
            .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = (
                01 00 00 00
            )
            IL_0000: ldarg.0
            IL_0001: ldarg.1
            IL_0002: stfld int32 OoBDev.Generations.Tests.TestTargets.ExampleCompleted::'<Abstract>k__BackingField'
            IL_0007: ret
        }
    }

    .method public hidebysig specialname rtspecialname instance void .ctor (
            class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext context
        ) cil managed 
    {
        IL_0000: ldarg.0
        IL_0001: call instance void OoBDev.Generations.Tests.TestTargets.IncompleteClassBase::.ctor()
        IL_0006: ldarg.0
        IL_0007: ldarg.1
        IL_0008: stfld class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext OoBDev.Generations.Tests.TestTargets.ExampleCompleted::_context
        IL_000d: ret
    }

    .method public hidebysig virtual instance string AbstractFunction () cil managed 
    {
        .locals init (
            [0] class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext V_0
        )

        IL_0000: ldarg.0
        IL_0001: ldfld class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext OoBDev.Generations.Tests.TestTargets.ExampleCompleted::_context
        IL_0006: callvirt instance class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext::get_Provider()
        IL_000b: ldarg.0
        IL_000c: ldfld class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext OoBDev.Generations.Tests.TestTargets.ExampleCompleted::_context
        IL_0011: ldtoken OoBDev.Generations.Tests.TestTargets.IncompleteClassBase
        IL_0016: call class [System.Runtime]System.Type [System.Runtime]System.Type::GetTypeFromHandle(valuetype [System.Runtime]System.RuntimeTypeHandle)
        IL_001b: ldstr "AbstractFunction"
        IL_0020: call instance class [System.Runtime]System.Reflection.MethodInfo [System.Runtime]System.Type::GetMethod(string)
        IL_0025: ldc.i4.0
        IL_0026: newarr [System.Runtime]System.Object
        IL_002b: callvirt instance class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider::CreateContext(class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext,  class [System.Runtime]System.Reflection.MethodBase,  object[])
        IL_0030: stloc.0
        IL_0031: ldloc.0
        IL_0032: callvirt instance class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext::get_Provider()
        IL_0037: ldloc.0
        IL_0038: callvirt instance object [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider::Generate(class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext)
        IL_003d: castclass [System.Runtime]System.String
        IL_0042: ret
    }

    .method public hidebysig virtual instance string AbstractFunction2 (
            string input1
        ) cil managed 
    {
        .locals init (
            [0] class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext V_0
        )

        IL_0000: ldarg.0
        IL_0001: ldfld class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext OoBDev.Generations.Tests.TestTargets.ExampleCompleted::_context
        IL_0006: callvirt instance class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext::get_Provider()
        IL_000b: ldarg.0
        IL_000c: ldfld class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext OoBDev.Generations.Tests.TestTargets.ExampleCompleted::_context
        IL_0011: ldtoken OoBDev.Generations.Tests.TestTargets.IncompleteClassBase
        IL_0016: call class [System.Runtime]System.Type [System.Runtime]System.Type::GetTypeFromHandle(valuetype [System.Runtime]System.RuntimeTypeHandle)
        IL_001b: ldstr "AbstractFunction2"
        IL_0020: call instance class [System.Runtime]System.Reflection.MethodInfo [System.Runtime]System.Type::GetMethod(string)
        IL_0025: ldc.i4.1
        IL_0026: newarr [System.Runtime]System.Object
        IL_002b: dup
        IL_002c: ldc.i4.0
        IL_002d: ldarg.1
        IL_002e: stelem.ref
        IL_002f: callvirt instance class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider::CreateContext(class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext,  class [System.Runtime]System.Reflection.MethodBase,  object[])
        IL_0034: stloc.0
        IL_0035: ldloc.0
        IL_0036: callvirt instance class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext::get_Provider()
        IL_003b: ldloc.0
        IL_003c: callvirt instance object [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider::Generate(class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext)
        IL_0041: castclass [System.Runtime]System.String
        IL_0046: ret
    }

    .method public hidebysig virtual instance void AbstractFunction3 (
            string input1,
            string input2
        ) cil managed 
    {
        .locals init (
            [0] class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext V_0
        )

        IL_0000: ldarg.0
        IL_0001: ldfld class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext OoBDev.Generations.Tests.TestTargets.ExampleCompleted::_context
        IL_0006: callvirt instance class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext::get_Provider()
        IL_000b: ldarg.0
        IL_000c: ldfld class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext OoBDev.Generations.Tests.TestTargets.ExampleCompleted::_context
        IL_0011: ldtoken OoBDev.Generations.Tests.TestTargets.IncompleteClassBase
        IL_0016: call class [System.Runtime]System.Type [System.Runtime]System.Type::GetTypeFromHandle(valuetype [System.Runtime]System.RuntimeTypeHandle)
        IL_001b: ldstr "AbstractFunction3"
        IL_0020: call instance class [System.Runtime]System.Reflection.MethodInfo [System.Runtime]System.Type::GetMethod(string)
        IL_0025: ldc.i4.2
        IL_0026: newarr [System.Runtime]System.Object
        IL_002b: dup
        IL_002c: ldc.i4.0
        IL_002d: ldarg.1
        IL_002e: stelem.ref
        IL_002f: dup
        IL_0030: ldc.i4.1
        IL_0031: ldarg.2
        IL_0032: stelem.ref
        IL_0033: callvirt instance class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider::CreateContext(class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext,  class [System.Runtime]System.Reflection.MethodBase,  object[])
        IL_0038: stloc.0
        IL_0039: ldloc.0
        IL_003a: callvirt instance class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext::get_Provider()
        IL_003f: ldloc.0
        IL_0040: callvirt instance object [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationProvider::Generate(class [OoBDev.Generations.Contracts]OoBDev.Generations.IProcedualGenerationContext)
        IL_0045: pop
        IL_0046: ret
    }
}
*/