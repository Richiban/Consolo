using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoLazy;

namespace Richiban.Cmdr
{
    internal class MethodHelp
    {
        public MethodHelp(
            string executableName,
            string routeHelp,
            string methodXmlComments,
            IReadOnlyList<ParameterHelp> parameterHelp)
        {
            ExecutableName = executableName;
            RouteHelp = routeHelp;
            MethodXmlComments = methodXmlComments;
            ParameterHelp = parameterHelp;
        }

        public string ExecutableName { get; }
        public string RouteHelp { get; }
        public string MethodXmlComments { get; }
        public IReadOnlyList<ParameterHelp> ParameterHelp { get; }

        [Lazy]
        public override string ToString()
        {
            var builder = new HelpStyleStringBuilder();

            var parameterHeadings = string.Join(" ", ParameterHelp);

            builder.AppendLine($@"{ExecutableName} {RouteHelp} {parameterHeadings}");

            using (builder.Indent())
            {
                if (!string.IsNullOrEmpty(MethodXmlComments))
                {
                    builder.AppendLine(MethodXmlComments);
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendLine();
                }

                var parameterHelpsToUse = ParameterHelp.Select(h => GetParameterHelp(h))
                    .Where(h => !string.IsNullOrEmpty(h));

                if (parameterHelpsToUse.Any())
                {
                    builder.AppendLine("Parameters:");
                }

                using (builder.Indent())
                {
                    foreach (var parameterHelp in parameterHelpsToUse)
                    {
                        builder.AppendLine(parameterHelp);
                        builder.AppendLine();
                    }
                }
            }

            return builder.ToString();
        }

        private string GetParameterHelp(ParameterHelp parameterHelp)
        {
            var isInterestingType = parameterHelp.Type != typeof(string) &&
                                    parameterHelp.Type != typeof(bool);

            var hasXmlComment = !string.IsNullOrEmpty(parameterHelp.XmlComments);

            if (isInterestingType == false && hasXmlComment == false)
            {
                return "";
            }

            var builder = new StringBuilder();
            builder.Append(parameterHelp);

            builder.Append(" ");
            builder.Append(parameterHelp.XmlComments);

            if (isInterestingType)
            {
                if (hasXmlComment)
                {
                    builder.Append(". ");
                }

                builder.Append("Type: ");
                builder.Append(parameterHelp.Type);
            }

            return builder.ToString();
        }
    }
}