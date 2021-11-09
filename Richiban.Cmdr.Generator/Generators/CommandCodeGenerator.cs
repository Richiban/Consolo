using System;
using System.Collections.Generic;
using System.Linq;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Writers;

namespace Richiban.Cmdr.Generators
{
    internal class CommandCodeGenerator : ICodeGenerator
    {
        private readonly CommandModel _commandModel;

        public CommandCodeGenerator(CommandModel commandModel)
        {
            _commandModel = commandModel;
        }

        public IEnumerable<string> GetCodeLines() =>  GetCode(_commandModel).Split('\n');
        
        private string GetCode(CommandModel commandModel) =>
            commandModel switch
            {
                CommandModel.RootCommandModel root => WriteRootString(root),
                CommandModel.CommandGroupModel group => WriteGroupString(@group),
                CommandModel.LeafCommandModel leaf => WriteLeafStringRec(leaf),
                _ => throw new InvalidOperationException(
                    $"Unknown {nameof(CommandModel)}: {_commandModel}")
            };

        private string WriteRootString(CommandModel.RootCommandModel rootCommandModel)
        {
            var leafCommandStatements = rootCommandModel.GetAllLeafCommandModels()
                .Select(WriteLeafString1)
                .StringJoin("\n");
            
            var subCommandExpressions = rootCommandModel.SubCommands.Select(GetCode)
                .StringJoin(",\n    ");

            return $@"
        {leafCommandStatements}

        var rootCommand = new RootCommand()
        {{
            {subCommandExpressions}
        }};";
        }

        private string WriteGroupString(CommandModel.CommandGroupModel commandGroupModel)
        {
            var subStrings = commandGroupModel.SubCommands.Select(GetCode)
                .StringJoin(",\n    ");

            return $@"new Command(""{commandGroupModel.CommandName}"") 
{{
    {subStrings}
}}";
        }

        private string WriteLeafStringRec(CommandModel.LeafCommandModel leafModel)
        {
            return leafModel.VariableName;
        }

        private string WriteLeafString1(
            CommandModel.LeafCommandModel leafModel)
        {
            var handlerString = GetHandlerString(leafModel);

            var parametersString = GetParametersString(leafModel);

            return $@"
var {leafModel.VariableName} = new Command(""{leafModel.CommandName}"")
{{ 
    {parametersString}
}};

{handlerString}";
        }

        private string GetParametersString(CommandModel.LeafCommandModel leafModel)
        {
            var parameterStrings = leafModel.Parameters.Select(ArgumentOrOptionToString);

            var parametersString = string.Join((string)",\n", parameterStrings);

            return parametersString;
        }

        private string GetHandlerString(CommandModel.LeafCommandModel leafModel)
        {
            var parameters = leafModel.Parameters;

            var handlerTypeArguments = parameters.Count == 0
                ? ""
                : $"<{parameters.Select(a => a.FullyQualifiedTypeName).StringJoin(", ")}>";

            return
                $"{leafModel.VariableName}.Handler = CommandHandler.Create{handlerTypeArguments}({leafModel.FullyQualifiedName});";
        }

        private string GenerateParentCommandExpression(
            CommandModel.CommandGroupModel group,
            string methodCommandName)
        {
            return group.SubCommands.Aggregate(
                methodCommandName,
                (current, cmd) => $@"new Command(""{cmd}"") {{ {current} }}");
        }

        private static string ArgumentOrOptionToString(
            CommandParameterModel commandParameterModel)
        {
            switch (commandParameterModel)
            {
                case CommandParameterModel.CommandPositionalParameterModel argument:
                    return ArgumentToString(argument);
                case CommandParameterModel.CommandFlagParameterModel option:
                    return OptionToString(option);
                default:
                    throw new InvalidOperationException(
                        $"Unknown {nameof(CommandParameterModel)}: {commandParameterModel}");
            }
        }

        private static string ArgumentToString(
            CommandParameterModel.CommandPositionalParameterModel argumentModel) =>
            $@"new Argument(""{argumentModel.Name}"")";

        private static string OptionToString(
            CommandParameterModel.CommandFlagParameterModel argumentModel)
        {
            var aliases = new[] { argumentModel.Name[0].ToString(), argumentModel.Name };

            var aliasesString = string.Join(", ", aliases.Select(a => $"\"{a}\""));

            return $@"new Option(new string[] {{{aliasesString}}})";
        }
    }
}