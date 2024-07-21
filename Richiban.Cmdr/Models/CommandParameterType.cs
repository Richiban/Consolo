using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr;

abstract class ParameterType
{
    private ParameterType() {}
    public abstract ITypeSymbol Symbol { get; }

    public string GetFullyQualifiedName() => Symbol.GetFullyQualifiedName();

    public sealed class AsIs(ITypeSymbol symbol) : ParameterType
    {
        public override ITypeSymbol Symbol { get; } = symbol;
    }

    public sealed class Constructor(ITypeSymbol symbol) : ParameterType
    {
        public override ITypeSymbol Symbol { get; } = symbol;
    }

    public sealed class ExplicitCast(ITypeSymbol symbol) : ParameterType
    {
        public override ITypeSymbol Symbol { get; } = symbol;
    }

    public sealed class Parse(ITypeSymbol symbol, string parseMethodName) : ParameterType
    {
        public override ITypeSymbol Symbol { get; } = symbol;
        public string ParseMethodName { get; } = parseMethodName;
    }
}