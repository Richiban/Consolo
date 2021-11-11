using System;
using System.Collections.Generic;
using System.Linq;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Transformers;
using Richiban.Cmdr.Utils;

namespace Richiban.Cmdr.Writers
{
    internal class ProgramClassFileGenerator : CodeFileGenerator
    {
        private readonly CodeBuilder _codeBuilder = new();
        private readonly CommandModel.RootCommandModel _commandModel;

        public ProgramClassFileGenerator(IReadOnlyCollection<MethodModel> methodModels)
        {
            _commandModel = new CommandModelTransformer().Transform(methodModels);
        }

        public override string FileName => "Program.g.cs";

        public override string GetCode()
        {
            _codeBuilder.AppendLines(
                "using System;",
                "using System.CommandLine;",
                "using System.CommandLine.Invocation;",
                "using Richiban.Cmdr;");

            _codeBuilder.AppendLine();

            _codeBuilder.AppendLines("public static class Program", "{");

            using (_codeBuilder.Indent())
            {
                _codeBuilder.AppendLines("public static int Main(string[] args)", "{");

                using (_codeBuilder.Indent())
                {
                    WriteLeafCommandStatements(_commandModel);
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

            return _codeBuilder.ToString();
        }

        private void WriteLeafCommandStatements(
            CommandModel.RootCommandModel rootCommandModel)
        {
            foreach (var leaf in rootCommandModel.GetAllLeafCommandModels())
            {
                _codeBuilder.AppendLine(
                    $"var {leaf.VariableName} = new Command(\"{leaf.CommandName}\")");

                _codeBuilder.AppendLine("{");

                using (_codeBuilder.Indent())
                {
                    WriteParameterExpressions(leaf);
                }

                _codeBuilder.AppendLine("};");
                _codeBuilder.AppendLine();

                WriteHandlerStatement(leaf);
                _codeBuilder.AppendLine();
            }
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

        private void WriteCommandExpression(
            CommandModel commandModel,
            CodeBuilder.CommaSeparatedExpressionSyntax expr)
        {
            switch (commandModel)
            {
                case CommandModel.CommandGroupModel group:
                    WriteGroupExpression(group, expr);

                    break;
                case CommandModel.LeafCommandModel leaf:
                    WriteLeafExpression(leaf, expr);

                    break;
                case CommandModel.RootCommandModel:
                    throw new InvalidOperationException(
                        "Writing the root command as an expression is not supported");
                default:
                    throw new InvalidOperationException(
                        $"Unknown {nameof(CommandModel)}: {_commandModel}");
            }
        }

        private void WriteRootStatement(CommandModel.RootCommandModel rootCommandModel)
        {
            _codeBuilder.AppendLine("var rootCommand = new RootCommand()");
            _codeBuilder.AppendLine("{");

            using (_codeBuilder.Indent())
            {
                using (var expr = _codeBuilder.OpenExpressionList())
                {
                    foreach (var subCommand in rootCommandModel.SubCommands)
                    {
                        WriteCommandExpression(subCommand, expr);
                    }
                }
            }

            _codeBuilder.AppendLine("};");
        }

        private void WriteGroupExpression(
            CommandModel.CommandGroupModel commandGroupModel,
            CodeBuilder.CommaSeparatedExpressionSyntax expr)
        {
            expr.AppendLine($"new Command(\"{commandGroupModel.CommandName}\")");
            expr.AppendLine("{");

            using (expr.Indent())
            {
                using (var expr2 = _codeBuilder.OpenExpressionList())
                {
                    foreach (var commandModel in commandGroupModel.SubCommands)
                    {
                        WriteCommandExpression(commandModel, expr2);
                        expr2.Next();
                    }
                }
            }

            expr.AppendLine("}");
            expr.Next();
        }

        private void WriteLeafExpression(
            CommandModel.LeafCommandModel leafModel,
            CodeBuilder.CommaSeparatedExpressionSyntax expr)
        {
            expr.AppendLine(leafModel.VariableName);
        }

        private void WriteParameterExpressions(CommandModel.LeafCommandModel leafModel)
        {
            using var expr = _codeBuilder.OpenExpressionList();

            foreach (var leafModelParameter in leafModel.Parameters)
            {
                expr.AppendLine(GetArgumentOrOptionExpression(leafModelParameter));
                expr.Next();
            }
        }

        private void WriteHandlerStatement(CommandModel.LeafCommandModel leafModel)
        {
            var parameters = leafModel.Parameters;

            var handlerTypeArguments = parameters.Count == 0
                ? ""
                : $"<{parameters.Select(a => a.FullyQualifiedTypeName).StringJoin(", ")}>";

            _codeBuilder.AppendLine(
                $"{leafModel.VariableName}.Handler = CommandHandler.Create{handlerTypeArguments}({leafModel.FullyQualifiedName});");
        }

        private static string GetArgumentOrOptionExpression(
            CommandParameterModel commandParameterModel)
        {
            switch (commandParameterModel)
            {
                case CommandParameterModel.CommandPositionalParameterModel argument:
                    return $@"new Argument(""{argument.Name}"")";
                case CommandParameterModel.CommandFlagParameterModel option:
                    var aliases = new[] { option.Name[index: 0].ToString(), option.Name };

                    var aliasesString = string.Join(
                        ", ",
                        aliases.Select(a => $"\"{a}\""));

                    return $@"new Option(new string[] {{{aliasesString}}})";
                default:
                    throw new InvalidOperationException(
                        $"Unknown {nameof(CommandParameterModel)}: {commandParameterModel}");
            }
        }
    }
}