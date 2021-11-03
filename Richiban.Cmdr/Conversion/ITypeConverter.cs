using System;
using System.Collections.Generic;

namespace Richiban.Cmdr
{
    /// <summary>
    /// Implementors of this interface are responsible for converting raw string values supplied on 
    /// the command line to the proper type for the parameter to the called method.
    /// 
    /// For optional parameters, the value <code>Type.Missing</code> should be returned.
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Try to convert a collection of strings into the type needed to then call the target method
        /// </summary>
        /// <param name="convertToType">The type of the target parameter</param>
        /// <param name="rawValues">The raw values supplied at the command line</param>
        /// <param name="convertedValue">The value converted, or Type.Missing, or null</param>
        /// <returns>True if the value was converted successfully</returns>
        bool TryConvertValue(
            Type convertToType, IReadOnlyList<string> rawValues, out object convertedValue);
    }
}
