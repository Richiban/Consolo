using AutoLazy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.CommandLine
{
    class ParameterHelp
    {
        public ParameterHelp(
            IReadOnlyList<ParameterName> parameterNames,
            bool isOptional,
            bool isFlag,
            Type type,
            string xmlComments)
        {
            ParameterNames = parameterNames;
            IsOptional = isOptional;
            IsFlag = isFlag;
            Type = type;
            XmlComments = xmlComments;
        }

        public IReadOnlyList<ParameterName> ParameterNames { get; }
        public bool IsOptional { get; }
        public bool IsFlag { get; }
        public Type Type { get; }
        public string XmlComments { get; }

        [Lazy]
        public override string ToString()
        {
            string parameterName(ParameterName pName)
            {
                return pName is ParameterName.ShortForm s
                    ? $"{CommandLineEnvironment.ShortFormFlagGlyph}{pName}"
                    : IsFlag
                        ? $"{CommandLineEnvironment.FlagGlyph}{pName}"
                        : $"<{pName}>";
            }

            var namesString = String.Join("|", ParameterNames.Select(parameterName));

            return IsOptional ? $"[{namesString}]" : namesString;
        }
    }
}
