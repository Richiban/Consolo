using System;
using System.Collections.Generic;
using System.Linq;
using AutoLazy;

namespace Richiban.Cmdr
{
    internal class ParameterHelp
    {
        public ParameterHelp(
            IReadOnlyList<ParameterName> parameterNames,
            bool isOptional,
            bool isFlag,
            bool allowMultipleValues,
            Type type,
            string xmlComments)
        {
            ParameterNames = parameterNames;
            IsOptional = isOptional;
            IsFlag = isFlag;
            AllowMultipleValues = allowMultipleValues;
            Type = type;
            XmlComments = xmlComments;
        }

        public IReadOnlyList<ParameterName> ParameterNames { get; }
        public bool IsOptional { get; }
        public bool IsFlag { get; }
        public bool AllowMultipleValues { get; }
        public Type Type { get; }
        public string XmlComments { get; }

        [Lazy]
        public override string ToString()
        {
            string parameterName(ParameterName pName) =>
                pName is ParameterName.ShortForm s
                    ?
                    $"-{pName}"
                    : IsFlag
                        ? $"-{pName}"
                        : $"<{pName}>";

            var namesString = string.Join("|", ParameterNames.Select(parameterName));

            if (AllowMultipleValues)
            {
                namesString = namesString + "...";
            }

            return IsOptional ? $"[{namesString}]" : namesString;
        }
    }
}