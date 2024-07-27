using System.Collections.Immutable;

namespace Consolo;

static class CommandParameterExtensions
{
    public static Option<ImmutableArray<EnumValue>> GetAllowedValues(this CommandParameter param) =>
        param switch
        {
            CommandParameter.Option { Type: ParameterType.Enum e } => e.EnumValues,
            _ => []
        };
}