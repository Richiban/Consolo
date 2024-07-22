using System;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Cmdr;

public static class SourceValueUtils
{
    public static string SourceValue(object? value) =>
        value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            _ => value.ToString() ?? "null"
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