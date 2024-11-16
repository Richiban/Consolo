using System;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Consolo;

public static class SourceValueUtils
{
    public static string SourceValue(object? value, ITypeSymbol typeSymbol) =>
        value switch
        {
            null => $"default({typeSymbol.GetFullyQualifiedName()})",
            string s => $"\"{s}\"",
            bool b => b ? "true" : "false",
            char c => $"'{c}'",
            int i when typeSymbol.TypeKind == TypeKind.Enum => $"({typeSymbol.GetFullyQualifiedName()}){value}",
            int i => i.ToString(),
            _ => throw new InvalidOperationException($"Unsupported parameter symbol: {typeSymbol.GetFullyQualifiedName()}"),
        };

    internal static string EscapeCSharpString(string str)
    {
        var sb = new StringBuilder();

        foreach (char c in str)
        {
            switch (c)
            {
                case '\\':
                    sb.Append("\\\\");

                    break;

                case '"':
                    sb.Append("\\\"");

                    break;

                case '\n':
                    sb.Append("\\n");

                    break;

                case '\r':
                    sb.Append("\\r");

                    break;

                case '\t':
                    sb.Append("\\t");

                    break;

                default:
                    // For other non-printable characters, use a unicode escape sequence
                    if (char.IsControl(c))
                    {
                        sb.AppendFormat("\\u{0:X4}", (int)c);
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    break;
            }
        }

        return sb.ToString();
    }
}