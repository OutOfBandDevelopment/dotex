using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// testable object type converter
/// </summary>
public interface IObjectConverter
{
    /// <summary>
    /// use best ability to convert from input to generic type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <returns></returns>
    T? Convert<T>(object? input) where T : class;

    /// <summary>
    /// use best ability to convert from input to target type
    /// </summary>
    /// <param name="input"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    object? Convert(object? input, Type target);
}
