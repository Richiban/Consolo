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

            if(AllowMultipleValues)
                namesString = namesString + "...";

            return IsOptional ? $"[{namesString}]" : namesString;
        }
    }
}
