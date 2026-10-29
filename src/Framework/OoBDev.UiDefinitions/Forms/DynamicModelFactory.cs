using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace OoBDev.UiDefinitions.Forms;

/// <summary>
/// Builds a plain model object with public auto-properties from a list at run time, so a form generated
/// from data (an instrument profile, a manifest) can use <see cref="FormBinding"/>, which reads models by reflection.
/// </summary>
public static class DynamicModelFactory
{
    private static readonly ModuleBuilder Module = AssemblyBuilder
        .DefineDynamicAssembly(new AssemblyName("OoBDev.UiDefinitions.DynamicModels"), AssemblyBuilderAccess.Run)
        .DefineDynamicModule("Models");

    private static int _counter;

    /// <summary>Creates a model with the given properties and sets each one's initial value.</summary>
    /// <param name="properties">The properties; names must be unique valid identifiers.</param>
    /// <returns>A new instance of a type created for this call.</returns>
    public static object Create(IEnumerable<DynamicProperty> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        var list = new List<DynamicProperty>(properties);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in list)
        {
            if (!seen.Add(property.Name))
            {
                throw new ArgumentException($"Duplicate property '{property.Name}'.", nameof(properties));
            }
        }

        var type = Module.DefineType($"Model{System.Threading.Interlocked.Increment(ref _counter)}", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);
        type.DefineDefaultConstructor(MethodAttributes.Public);
        foreach (var property in list)
        {
            Define(type, property);
        }

        var instance = Activator.CreateInstance(type.CreateType())!;
        foreach (var property in list.Where(p => p.Initial is not null))
        {
            instance.GetType().GetProperty(property.Name)!.SetValue(instance, property.Initial);
        }

        return instance;
    }

    private static void Define(TypeBuilder type, DynamicProperty property)
    {
        var field = type.DefineField("_" + property.Name, property.Type, FieldAttributes.Private);
        var builder = type.DefineProperty(property.Name, PropertyAttributes.None, property.Type, null);
        const MethodAttributes accessor = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        var getter = type.DefineMethod("get_" + property.Name, accessor, property.Type, Type.EmptyTypes);
        var get = getter.GetILGenerator();
        get.Emit(OpCodes.Ldarg_0);
        get.Emit(OpCodes.Ldfld, field);
        get.Emit(OpCodes.Ret);

        var setter = type.DefineMethod("set_" + property.Name, accessor, null, [property.Type]);
        var set = setter.GetILGenerator();
        set.Emit(OpCodes.Ldarg_0);
        set.Emit(OpCodes.Ldarg_1);
        set.Emit(OpCodes.Stfld, field);
        set.Emit(OpCodes.Ret);

        builder.SetGetMethod(getter);
        builder.SetSetMethod(setter);
    }
}
