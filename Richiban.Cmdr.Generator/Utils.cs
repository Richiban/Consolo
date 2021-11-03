using System;
using System.Text;

namespace Richiban.Cmdr.Generator
{
    public static class Utils
    {
        public static string ToKebabCase(string text)
        {
            var sb = new StringBuilder(text.Length);
            var first = true;

            foreach (var c in text)
            {
                if (char.IsUpper(c))
                {
                    if (!first)
                        sb.Append('-');

                    sb.Append(char.ToLower(c));
                }
                else
                {
                    sb.Append(c);
                }

                first = false;
            }

            return sb.ToString();
        }
    }
}