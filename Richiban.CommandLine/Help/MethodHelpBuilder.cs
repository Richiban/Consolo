using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Richiban.CommandLine
{
    class MethodHelpBuilder
    {
        private readonly XmlCommentsRepository _xmlComments;

        public MethodHelpBuilder(XmlCommentsRepository xmlComments)
        {
            _xmlComments = xmlComments;
        }

        public MethodHelp BuildFor(MethodModel method)
        {
            return new MethodHelp(
                AppDomain.CurrentDomain.FriendlyName,
                HelpForRoutes(method.Routes),
                HelpForParameters(method.Parameters),
                _xmlComments[method]);
        }

        private string HelpForParameters(ParameterModelList parameters)
        {
            return String.Join(" ", parameters.Select(buildHelpForParameter));

            string buildHelpForParameter(ParameterModel p)
            {
                var helpForm =
                    p.IsFlag
                    ? $"{CommandLineEnvironment.FlagGlyph}{p.Name.ToLowerInvariant()}"
                    : $"<{p.Name.ToLowerInvariant()}>";

                return p.IsOptional ? $"[{helpForm}]" : helpForm;
            }
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

        //private static string GenerateHelp(IEnumerable<MethodModel> model, CommandLineArgumentList commandLineArgs)
        //{
        //    var sb = new StringBuilder();

        //    var modelsForHelp = model;

        //    if (commandLineArgs.Count == 0)
        //    {
        //        sb.AppendLine($"Usage:");
        //    }
        //    else
        //    {
        //        sb.AppendLine($"Help for {commandLineArgs}:");

        //        modelsForHelp = modelsForHelp
        //            .AllByMax(m => m.GetPartialMatchAccuracy(commandLineArgs));
        //    }

        //    sb.Append(
        //        string.Join($"{Environment.NewLine}{Environment.NewLine}",
        //        modelsForHelp
        //        .Select(t => $"\t{AppDomain.CurrentDomain.FriendlyName} {t.Help}")));

        //    return sb.ToString();
        //}
    }
}
