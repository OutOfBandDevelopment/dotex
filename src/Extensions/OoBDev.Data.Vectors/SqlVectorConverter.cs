using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;

namespace OoBDev.Data.Vectors;

public class SqlVectorConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
        new Type[]
        {
            typeof(string),
            typeof(SqlString),
            typeof(float[]),
            typeof(double[]),
            typeof(SqlVectorF[]),
            typeof(SqlVector[]),
            typeof(byte[]),
            typeof(SqlBinary),
        }.Contains(sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
        value switch
        {
            null => SqlVector.Null,
            INullable nullable when nullable.IsNull => SqlVector.Null,
            string data => SqlVector.Parse(new SqlString(data)),
            SqlString data => SqlVector.Parse(data),
            float[] data => new SqlVector(data),
            double[] data => new SqlVector(data),
            SqlVectorF data => (SqlVector)data,
            SqlVector data => data,
            byte[] data => SqlVector.From(data),
            SqlBinary data => SqlVector.From(data.Value),
            _ => throw new NotSupportedException($"{value.GetType()} is not supported")
        };

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
        new Type[]
        {
            typeof(string),
            typeof(float[]),
            typeof(double[]),
            typeof(SqlVectorF[]),
            typeof(SqlVector[]),
            typeof(byte[]),
            typeof(SqlBinary),
        }.Contains(destinationType);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (!(value is SqlVector vector)) throw new NotSupportedException($"{value.GetType()} is not supported");

        if (vector == null) return SqlVector.Null;
        else if (destinationType == typeof(string)) return vector.IsNull ? null : vector.ToString();
        else if (destinationType == typeof(SqlString)) return vector.IsNull ? SqlString.Null : new SqlString(vector.ToString());
        else if (destinationType == typeof(float[])) return vector.IsNull ? null : vector.Values.Select(Convert.ToSingle).ToArray();
        else if (destinationType == typeof(SqlVectorF)) return vector.IsNull ? SqlVectorF.Null : (SqlVectorF)vector;
        else if (destinationType == typeof(double[])) return vector.IsNull ? null : vector.Values;
        else if (destinationType == typeof(SqlVector)) return vector.IsNull ? SqlVector.Null : vector;
        else if (destinationType == typeof(byte[])) return vector.IsNull ? null : vector.ToBytes();
        else if (destinationType == typeof(SqlBinary)) return vector.IsNull ? SqlBinary.Null : new SqlBinary(vector.ToBytes());

        throw new NotSupportedException($"{destinationType} is not supported");
    }
}
