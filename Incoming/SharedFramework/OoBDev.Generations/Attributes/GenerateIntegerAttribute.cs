using System;
using System.Linq;

namespace OoBDev.Generations.Attributes
{
    public class GenerateIntegerAttribute : GenerateNullableAttribute
    {
        public override bool CanGenerateValue(IProcedualGenerationContext context) =>
            new[] {
                typeof(int), typeof(int?),
                typeof(uint), typeof(uint?),
                typeof(byte), typeof(byte?),
                typeof(sbyte), typeof(sbyte?),
                typeof(short), typeof(short?),
                typeof(ushort), typeof(ushort?),
            }.Contains(context.TargetType);

        protected override object? OnGenerateValue(IProcedualGenerationContext context)
        {
            var result = context.Random.Next();
            if (context.TargetType == typeof(uint) || context.TargetType == typeof(uint?)) return (uint)result;
            if (context.TargetType == typeof(byte) || context.TargetType == typeof(byte?)) return (byte)result;
            if (context.TargetType == typeof(sbyte) || context.TargetType == typeof(sbyte?)) return (sbyte)result;
            if (context.TargetType == typeof(short) || context.TargetType == typeof(short?)) return (short)result;
            if (context.TargetType == typeof(ushort) || context.TargetType == typeof(ushort?)) return (ushort)result;
            else return result;
        }
    }
}
