using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;

namespace OoBDev.Data.Vectors;

public class SqlVectorFConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
        new Type[]
        {
            typeof(string),
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
            string data => SqlVectorF.Parse(new SqlString(data)),
            float[] data => new SqlVectorF(data),
            double[] data => new SqlVectorF(data),
            SqlVectorF data => data,
            SqlVector data => (SqlVectorF)data,
            byte[] data => SqlVectorF.From(data),
            SqlBinary data => SqlVectorF.From(data.Value),
            _ => SqlVector.Null
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
        if (!(value is SqlVectorF vector)) throw new NotSupportedException($"{value.GetType()} is not supported");

        if (vector == null) return SqlVector.Null;
        else if (destinationType == typeof(string)) return vector.IsNull ? null : vector.ToString();
        else if (destinationType == typeof(SqlString)) return vector.IsNull ? SqlString.Null : new SqlString(vector.ToString());
        else if (destinationType == typeof(float[])) return vector.IsNull ? null : vector.Values.Select(Convert.ToSingle).ToArray();
        else if (destinationType == typeof(SqlVectorF)) return vector.IsNull ? SqlVectorF.Null : vector;
        else if (destinationType == typeof(double[])) return vector.IsNull ? null : vector.Values;
        else if (destinationType == typeof(SqlVector)) return vector.IsNull ? SqlVector.Null : (SqlVector)vector;
        else if (destinationType == typeof(byte[])) return vector.IsNull ? null : vector.ToBytes();
        else if (destinationType == typeof(SqlBinary)) return vector.IsNull ? SqlBinary.Null : new SqlBinary(vector.ToBytes());

        throw new NotSupportedException($"{destinationType} is not supported");
    }
}
