using System;
using System.Collections.Generic;

namespace Cmdr;

public static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dictionary, 
        TKey key, 
        TValue defaultValue)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        return defaultValue;
    }
}
