using System;
using System.Collections.Generic;
using System.Linq;

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
