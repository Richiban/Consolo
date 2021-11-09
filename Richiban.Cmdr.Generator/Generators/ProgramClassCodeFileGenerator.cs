using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Transformers;
using Richiban.Cmdr.Writers;

namespace Richiban.Cmdr.Generators
{
    internal class ProgramClassCodeFileGenerator : ICodeFileGenerator
    {
        private readonly CodeBuilder _codeBuilder = new();
        private readonly CommandModel.RootCommandModel _commandModel;

        public ProgramClassCodeFileGenerator(
            IReadOnlyCollection<MethodModel> methodModels)
        {
            _commandModel = new CommandModelTransformer().Transform(methodModels);
        }

        public string GetCode()
        {
            WriteCodeLines();

            return _codeBuilder.ToString();
        }

        private void WriteCodeLines()
        {
            _codeBuilder.AppendLine("using System;");
            _codeBuilder.AppendLine("using System.CommandLine;");
            _codeBuilder.AppendLine("using System.CommandLine.Invocation;");
            _codeBuilder.AppendLine("using Richiban.Cmdr;");

            _codeBuilder.AppendLine();

            _codeBuilder.AppendLines("public static class Program", "{");

            using (_codeBuilder.Indent())
            {
                _codeBuilder.AppendLines("public static int Main(string[] args)", "{");

                using (_codeBuilder.Indent())
                {
                    WriteRootStatement(_commandModel);

                    _codeBuilder.AppendLine();

                    _codeBuilder.AppendLines(
                        "if (args.Length == 1 && (args[0] == \"--interactive\" || args[0] == \"-i\"))",
                        "{");

                    WriteReplCall();

                    _codeBuilder.AppendLine("}", "else", "{");

                    WriteRootCommandCall();

                    _codeBuilder.AppendLine("}");
                }

                _codeBuilder.AppendLine("}");
            }

            _codeBuilder.AppendLine("}");
        }

        private void WriteRootCommandCall()
        {
            using (_codeBuilder.Indent())
            {
                _codeBuilder.AppendLine("return rootCommand.Invoke(args);");
            }
        }

        private void WriteReplCall()
        {
            using (_codeBuilder.Indent())
            {
                _codeBuilder.AppendLine(
                    "var repl = new Repl(rootCommand, \"Select a command\");");

                _codeBuilder.AppendLine("repl.EnterLoop();");
                _codeBuilder.AppendLine();
                _codeBuilder.AppendLine("return 0;");
            }
        }

        private void WriteCommandExpression(CommandModel commandModel)
        {
            switch (commandModel)
            {
                case CommandModel.CommandGroupModel group:
                    WriteGroupExpression(group);

                    break;
                case CommandModel.LeafCommandModel leaf:
                    WriteLeafExpression(leaf);

                    break;
                case CommandModel.RootCommandModel:
                    throw new InvalidOperationException(
                        $"Writing the root command as an expression is not supported");
                default:
                    throw new InvalidOperationException(
                        $"Unknown {nameof(CommandModel)}: {_commandModel}");
            }
        }

        private void WriteRootStatement(CommandModel.RootCommandModel rootCommandModel)
        {
            foreach (var leaf in rootCommandModel.GetAllLeafCommandModels())
            {
                WriteLeafStatement(leaf);
            }

            _codeBuilder.AppendLine("var rootCommand = new RootCommand()");
            _codeBuilder.AppendLine("{");

            using (var expr = _codeBuilder.OpenExpressionList())
            {
                foreach (var subCommand in rootCommandModel.SubCommands)
                {
                    //expr.Append(/*subCommand*/ "[This is supposed to be a subcommand]");
                    WriteCommandExpression(subCommand);
                }
            }

            _codeBuilder.AppendLine("};");
        }

        private void WriteGroupExpression(
            CommandModel.CommandGroupModel commandGroupModel)
        {
            _codeBuilder.AppendLine($"new Command(\"{commandGroupModel.CommandName}\")");
            _codeBuilder.AppendLine("{");

            using (_codeBuilder.Indent())
            {
                foreach (var commandModel in commandGroupModel.SubCommands)
                {
                    WriteCommandExpression(commandModel);
                }
            }

            _codeBuilder.AppendLine("}");
        }

        private void WriteLeafExpression(CommandModel.LeafCommandModel leafModel)
        {
            _codeBuilder.AppendLine(leafModel.VariableName);
        }

        private void WriteLeafStatement(CommandModel.LeafCommandModel leafModel)
        {
            _codeBuilder.AppendLine(
                $"var {leafModel.VariableName} = new Command(\"{leafModel.CommandName}\")");

            _codeBuilder.AppendLine("{");

            GetParametersString(leafModel);

            _codeBuilder.AppendLine("};");
            _codeBuilder.AppendLine();

            WriteHandlerString(leafModel);
            _codeBuilder.AppendLine();
        }

        private void GetParametersString(CommandModel.LeafCommandModel leafModel)
        {
            var parameterStrings = leafModel.Parameters.Select(ArgumentOrOptionToString);

            var parametersString = string.Join((string)",\n", parameterStrings);

            _codeBuilder.AppendLine(parametersString);
        }

        private void WriteHandlerString(CommandModel.LeafCommandModel leafModel)
        {
            var parameters = leafModel.Parameters;

            var handlerTypeArguments = parameters.Count == 0
                ? ""
                : $"<{parameters.Select(a => a.FullyQualifiedTypeName).StringJoin(", ")}>";

            _codeBuilder.AppendLine(
                $"{leafModel.VariableName}.Handler = CommandHandler.Create{handlerTypeArguments}({leafModel.FullyQualifiedName});");
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