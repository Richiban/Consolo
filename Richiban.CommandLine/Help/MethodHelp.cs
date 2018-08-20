using AutoLazy;
using System.Text;

namespace Richiban.CommandLine
{
    class MethodHelp
    {
        public MethodHelp(
            string executableName,
            string routeHelp,
            string parameterHelp,
            Option<XmlComments> xmlComments)
        {
            ExecutableName = executableName;
            RouteHelp = routeHelp;
            ParameterHelp = parameterHelp;
            XmlComments = xmlComments;
        }

        public string ExecutableName { get; }
        public string RouteHelp { get; }
        public string ParameterHelp { get; }
        public Option<XmlComments> XmlComments { get; }

        [Lazy]
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($@"{ExecutableName} {RouteHelp} {ParameterHelp}");

            XmlComments.IfSome(x => {
                sb.Append("\t");
                sb.AppendLine(x.MethodComments);

                foreach(var kv in x.ParameterComments)
                {
                    sb.AppendLine();
                    sb.Append("\t");
                    sb.AppendLine(kv.Key);
                    sb.Append("\t");
                    sb.Append("\t");
                    sb.AppendLine(kv.Value);
                }
            });

            return sb.ToString();
        }
    }
}
