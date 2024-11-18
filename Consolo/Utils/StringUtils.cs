﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Consolo;

public static class StringUtils
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
                {
                    sb.Append(value: '-');
                }

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

    public static string ToCamelCase(string s)
    {
        if (String.IsNullOrEmpty(s))
        {
            return s;
        }

        var goLower = true;
        var goUpper = false;

        IEnumerable<char> getChars()
        {
            foreach (var c in s)
            {
                if (goLower)
                {
                    yield return char.ToLower(c);

                    goLower = false;
                    goUpper = false;
                }
                else if (goUpper)
                {
                    yield return char.ToUpper(c);

                    goLower = false;
                    goUpper = false;
                }
                else
                {
                    if (c == '-')
                    {
                        goUpper = true;
                    }
                    else
                    {
                        yield return c;
                    }
                }
            }
        }

        return new string(getChars().ToArray());
    }
}