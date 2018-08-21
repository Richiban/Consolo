using AutoLazy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Richiban.CommandLine
{
    class MethodHelp
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
            var builder = new HelpStringBuilder();

            var parameterHeadings = String.Join(" ", ParameterHelp.Select(p => p.Heading));

            builder.AppendLine($@"{ExecutableName} {RouteHelp} {parameterHeadings}");

            using (builder.Indent())
            {
                if (!String.IsNullOrEmpty(MethodXmlComments))
                {
                    builder.AppendLine(MethodXmlComments);
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendLine();
                }

                var parameterHelpsToUse = ParameterHelp
                    .Select(h => GetParameterHelp(h))
                    .Where(h => !String.IsNullOrEmpty(h));

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
            var isInterestingType = parameterHelp.Type != typeof(string) && parameterHelp.Type != typeof(bool);
            var hasXmlComment = !String.IsNullOrEmpty(parameterHelp.XmlComments);

            if (isInterestingType == false && hasXmlComment == false)
                return "";

            var builder = new StringBuilder();
            builder.Append(parameterHelp.Heading);

            builder.Append(" ");
            builder.Append(parameterHelp.XmlComments);

            if (isInterestingType)
            {
                if (hasXmlComment)
                {
                    builder.Append(". ");
                }

                builder.Append("Type: ");
                builder.Append(parameterHelp.Type.ToString());
            }

            return builder.ToString();
        }
    }
}
