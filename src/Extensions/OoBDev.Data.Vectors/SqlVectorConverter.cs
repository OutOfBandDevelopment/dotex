using System;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;

namespace OoBDev.Data.Vectors;

/// <summary>
/// Provides type conversion for <see cref="SqlVector"/> to and from various formats including strings, arrays, and binary data.
/// </summary>
public class SqlVectorConverter : TypeConverter
{
    /// <summary>
    /// Determines whether this converter can convert from the specified source type.
    /// </summary>
    /// <param name="context">The type descriptor context.</param>
    /// <param name="sourceType">The source type to convert from.</param>
    /// <returns>True if conversion is supported; otherwise, false.</returns>
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

    /// <summary>
    /// Converts the specified value to a <see cref="SqlVector"/>.
    /// </summary>
    /// <param name="context">The type descriptor context.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted <see cref="SqlVector"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when the value type is not supported.</exception>
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

    /// <summary>
    /// Determines whether this converter can convert to the specified destination type.
    /// </summary>
    /// <param name="context">The type descriptor context.</param>
    /// <param name="destinationType">The destination type to convert to.</param>
    /// <returns>True if conversion is supported; otherwise, false.</returns>
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

    /// <summary>
    /// Converts a <see cref="SqlVector"/> to the specified destination type.
    /// </summary>
    /// <param name="context">The type descriptor context.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <param name="value">The <see cref="SqlVector"/> to convert.</param>
    /// <param name="destinationType">The type to convert to.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotSupportedException">Thrown when the value or destination type is not supported.</exception>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (!(value is SqlVector vector)) throw new NotSupportedException($"{value.GetType()} is not supported");

        if (vector.IsNull) return SqlVector.Null;
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
