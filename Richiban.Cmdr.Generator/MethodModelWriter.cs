using System;
using System.Collections.Generic;
using System.Linq;

namespace Richiban.Cmdr.Generator
{
    class MethodModelWriter
    {
        private readonly MethodModel _methodModel;

        public MethodModelWriter(MethodModel methodModel)
        {
            _methodModel = methodModel;
        }

        public string WriteString()
        {
            var parameterStrings =
                _methodModel.Arguments.Select(ArgumentOrOptionToString);

            var parametersString = string.Join((string)",\n", parameterStrings);

            var argumentTypes = _methodModel.Arguments.Select(a => a.FullyQualifiedTypeName);

            var argumentTypesString = string.Join(", ", argumentTypes);

            var cmdName = _methodModel.NameIn + "Command";

            return $@"
var {cmdName} = new Command(""{_methodModel.NameOut}"")
{{ 
    {parametersString}
}};

{cmdName}.Handler = CommandHandler.Create<{argumentTypesString}>({_methodModel.FullyQualifiedClassName}.{_methodModel.NameIn});

rootCommand.Add({cmdName});";
        }

        private static string ArgumentOrOptionToString(ArgumentModel x)
        {
            return x.IsFlag ? OptionToString(x) : ArgumentToString(x);
        }

        private static string ArgumentToString(ArgumentModel x) =>
            $@"new Argument(""{x.NameOut}"")";

        private static string OptionToString(ArgumentModel argumentModel)
        {
            var aliases = new[]
            {
                argumentModel.NameOut[0].ToString(), argumentModel.NameOut
            };

            var aliasesString = string.Join(", ", aliases.Select(a => $"\"{a}\""));

            return $@"new Option(new string[] {{{aliasesString}}})";
        }
    }
}