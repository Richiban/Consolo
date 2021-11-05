using System;
using System.Linq;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr.Writers
{
    class CommandModelWriter
    {
        private readonly CommandModel _commandModel;

        public CommandModelWriter(CommandModel commandModel)
        {
            _commandModel = commandModel;
        }

        public string WriteString()
        {
            var cmdName = _commandModel.Name;

            var handlerString = GetHandlerString(cmdName);

            var parentCommandExpression =
                GenerateParentCommandExpression(_methodModel, cmdName);

            var parametersString = GetParametersString();

            return $@"
var {cmdName} = new Command(""{_methodModel.NameOut}"")
{{ 
    {parametersString}
}};

{handlerString}

rootCommand.Add({parentCommandExpression});";
        }

        private string GetParametersString()
        {
            var parameterStrings =
                _methodModel.Arguments.Select(ArgumentOrOptionToString);

            var parametersString = string.Join((string)",\n", parameterStrings);

            return parametersString;
        }

        private string GetHandlerString(string methodCommandName)
        {
            var arguments = _methodModel.Arguments;

            var handlerTypeArguments = arguments.Count == 0
                ? ""
                : $"<{arguments.Select(a => a.FullyQualifiedTypeName).StringJoin(", ")}>";

            return
                $"{methodCommandName}.Handler = CommandHandler.Create{handlerTypeArguments}({_methodModel.FullyQualifiedClassName}.{_methodModel.Name});";
        }

        private string GenerateParentCommandExpression(
            MethodModel methodModel,
            string methodCommandName)
        {
            return methodModel.ParentNames.Aggregate(
                methodCommandName,
                (current, cmd) => $@"new Command(""{cmd}"") {{ {current} }}");
        }

        private static string ArgumentOrOptionToString(ArgumentModel argumentModel)
        {
            return argumentModel.IsFlag
                ? OptionToString(argumentModel)
                : ArgumentToString(argumentModel);
        }

        private static string ArgumentToString(ArgumentModel argumentModel) =>
            $@"new Argument(""{argumentModel.NameOut}"")";

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