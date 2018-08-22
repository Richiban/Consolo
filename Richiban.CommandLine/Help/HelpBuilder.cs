using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Richiban.CommandLine
{
    class HelpBuilder
    {
        private readonly XmlCommentsRepository _xmlComments;

        public HelpBuilder(XmlCommentsRepository xmlComments)
        {
            _xmlComments = xmlComments;
        }

        public string GenerateHelp(
            CommandLineArgumentList commandLineArgs,
            IEnumerable<MethodModel> models,
            IReadOnlyCollection<CommandLineAction> resolvedCommandLineActions)
        {
            var modelsForHelp = models.AllByMax(m => m.GetPartialMatchAccuracy(commandLineArgs));

            var sb = new StringBuilder();

            if (!commandLineArgs.IsCallForHelp)
            {
                if (resolvedCommandLineActions.Count == 0)
                {
                    sb.AppendLine("Could not match the given arguments to a command");
                    sb.AppendLine("");
                }
                else if (resolvedCommandLineActions.Count > 1)
                {
                    sb.AppendLine("The given arguments are ambiguous between the following:");

                    modelsForHelp = resolvedCommandLineActions.Select(a => a.Model);
                }
            }

            if (commandLineArgs.Count == 0)
            {
                sb.AppendLine("Usage:");
            }
            else
            {
                sb.AppendLine($"Help for {commandLineArgs}:");
            }

            sb.AppendLine("");

            foreach(var model in modelsForHelp)
            {
                sb.AppendLine(BuildForMethod(model).ToString());
            }

            return sb.ToString();
        }

        private MethodHelp BuildForMethod(MethodModel method)
        {
            var xmlComments = _xmlComments[method];

            return new MethodHelp(
                AppDomain.CurrentDomain.FriendlyName,
                HelpForRoutes(method.Routes),
                xmlComments.Match(Some: x => x.MethodComments, None: () => ""),
                method.Parameters.Select(p => new ParameterHelp(
                    p.Names,
                    p.IsOptional,
                    p.IsFlag,
                    p.ParameterType,
                    GetCommentsForParameterOrEmpty(p.OriginalName, xmlComments))).ToList());
        }

        private string GetCommentsForParameterOrEmpty(string parameterName, Option<XmlComments> xmlComments)
        {
            return
                xmlComments.Match(
                    Some: x => x.ParameterComments.ContainsKey(parameterName)
                        ? x.ParameterComments[parameterName]
                        : "",
                    None: () => "");
        }

        private string HelpForRoutes(RouteCollection routes)
        {
            var helpItems = routes.Select(buildHelpForSequence);

            return String.Join("|", helpItems);

            string buildHelpForSequence(IReadOnlyList<Verb> verbSequence)
            {
                return String.Join(" ", verbSequence);
            }
        }
    }
}
