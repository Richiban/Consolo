using System;
using System.Linq;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Utils;

namespace Richiban.Cmdr.Writers
{
    internal class ProgramClassFileGenerator : CodeFileGenerator
    {
        private readonly CodeBuilder _codeBuilder = new();
        private readonly CommandModel.RootCommandModel _commandModel;

        public ProgramClassFileGenerator(CommandModel.RootCommandModel commandModel)
        {
            _commandModel = commandModel;
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
                    WriteHandledCommandStatements(_commandModel);
                    WriteRootStatement(_commandModel);

                    _codeBuilder.AppendLine();

                    _codeBuilder.AppendLines("if (Repl.IsCall(args))", "{");

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

        private void WriteHandledCommandStatements(
            CommandModel.RootCommandModel rootCommandModel)
        {
            foreach (var command in rootCommandModel.GetDescendentCommands()
                .Where(x => x.Method != null).Reverse())
            {
                _codeBuilder.AppendLine(
                    $"var {command.GetVariableName()} = new Command(\"{command.CommandName}\", description: \"{command.Description}\")");

                _codeBuilder.AppendLine("{");

                using (_codeBuilder.Indent())
                {
                    using (var expr = _codeBuilder.OpenExpressionList())
                    {
                        foreach (var subCommand in command.SubCommands)
                        {
                            WriteCommandExpression(subCommand, expr);
                            expr.Next();
                        }

                        WriteParameterExpressions(command, expr);
                    }
                }

                _codeBuilder.AppendLine("};");
                _codeBuilder.AppendLine();

                WriteHandlerStatement(command);
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
                    "Repl.EnterNewLoop(rootCommand, \"Select a command\");");

                _codeBuilder.AppendLine();
                _codeBuilder.AppendLine("return 0;");
            }
        }

        private void WriteCommandExpression(
            CommandModel.SubCommandModel commandModel,
            CodeBuilder.CommaSeparatedExpressionSyntax expr)
        {
            if (commandModel.Method == null)
            {
                WriteImmediateCommandExpression(commandModel, expr);
            }
            else
            {
                WriteVariableCommandExpression(commandModel, expr);
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
                        expr.Next();
                    }
                }
            }

            _codeBuilder.AppendLine("};");

            if (rootCommandModel.Method != null)
            {
                _codeBuilder.AppendLine();
                WriteHandlerStatement(rootCommandModel);
            }
        }

        private void WriteImmediateCommandExpression(
            CommandModel.SubCommandModel commandGroupModel,
            CodeBuilder.CommaSeparatedExpressionSyntax expr)
        {
            expr.AppendLine($"new Command(\"{commandGroupModel.CommandName}\", description: \"{commandGroupModel.Description}\")");
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

        private void WriteVariableCommandExpression(
            CommandModel.SubCommandModel subModel,
            CodeBuilder.CommaSeparatedExpressionSyntax expr)
        {
            expr.AppendLine(subModel.GetVariableName());
        }

        private void WriteParameterExpressions(
            CommandModel.SubCommandModel subModel,
            CodeBuilder.CommaSeparatedExpressionSyntax expr)
        {
            if (subModel.Method == null)
            {
                return;
            }

            foreach (var leafModelParameter in subModel.Method.Parameters)
            {
                expr.AppendLine(GetArgumentOrOptionExpression(leafModelParameter));
                expr.Next();
            }
        }

        private void WriteHandlerStatement(CommandModel normalModel)
        {
            if (normalModel.Method is null)
            {
                return;
            }

            var parameters = normalModel.Method.Parameters;

            var handlerTypeArguments = parameters.Count == 0
                ? ""
                : $"<{parameters.Select(a => a.FullyQualifiedTypeName).StringJoin(", ")}>";

            var fullyQualifiedName = normalModel.Method.FullyQualifiedName;

            _codeBuilder.AppendLine(
                $"{normalModel.GetVariableName()}.Handler = CommandHandler.Create{handlerTypeArguments}({fullyQualifiedName});");
        }

        private static string GetArgumentOrOptionExpression(
            CommandParameterModel commandParameterModel) =>
            commandParameterModel switch
            {
                CommandParameterModel.CommandPositionalParameterModel argument => argument.IsRequired
                                        ? $"""
                            new Argument<{argument.FullyQualifiedTypeName}>(
                                name: "{argument.Name}",
                                description: "{argument.Description}")
                            """
                                        : $"""
                            new Argument<{argument.FullyQualifiedTypeName}>(
                                name: "{argument.Name}",
                                description: "{argument.Description}",
                                getDefaultValue: () => {argument.DefaultValue ?? "default"})
                            """,
                CommandParameterModel.CommandFlagModel option => 
                    $$"""
                    new Option(new [] {"-{{option.Name[index: 0]}}", "--{{option.Name}}"}, description: "{{option.Description}}")
                    """,
                _ => throw new UnknownCommandParameterModelException(commandParameterModel),
            };

        private class UnknownCommandParameterModelException : Exception 
        {
            public UnknownCommandParameterModelException(CommandParameterModel commandParameterModel) 
                : base(GetMessage(commandParameterModel)) { }

            private static string GetMessage(CommandParameterModel commandParameterModel) 
            {
                return $"Unknown {nameof(CommandParameterModel)}: {commandParameterModel}";
            }
        }
    }
}