using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Consolo;

static class ParameterModelExtensions
{
    public static IEnumerable<string> GetAllNames(this ParameterModel model)
    {
        yield return model.Name;

        if (model.Alias.IsSome(out var alias))
        {
            yield return alias;
        }
    }
}