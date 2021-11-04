using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Richiban.Cmdr.Generator
{
    [Generator]
    public class CmdrGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                AddAttribute(context);

                var components = GetComponents(context.Compilation);

                AddMainMethod(context, components);
            }
            catch (Exception)
            {
                Debugger.Launch();
            }
        }

        private static ImmutableArray<MethodModel> GetComponents(Compilation compilation)
        {
            var allNodes =
                compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());

            var allClasses = allNodes.Where(d => d.IsKind(SyntaxKind.MethodDeclaration))
                .OfType<MethodDeclarationSyntax>();

            return allClasses.Choose(component => TryGetComponent(compilation, component))
                .ToImmutableArray();
        }

        private static MethodModel? TryGetComponent(
            Compilation compilation,
            MethodDeclarationSyntax method)
        {
            var attributes = method.AttributeLists.SelectMany(x => x.Attributes)
                .Where(attr => attr.Name.ToString().Contains("CmdrMethod"))
                .ToList();

            if (!attributes.Any())
            {
                return null;
            }

            var methodName = method.Identifier.Text;

            var parameters = method.ParameterList.Parameters.Select(GetArgumentModel)
                .ToArray();

            var classDeclarationSyntax = (ClassDeclarationSyntax)method.Parent;

            var usings = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree)
                .GetDeclaredSymbol(classDeclarationSyntax)
                .DeclaringSyntaxReferences
                .SelectMany(x => x.SyntaxTree.GetCompilationUnitRoot().Usings)
                .Select(x => x.Name.ToString())
                .ToArray();

            var classNamespace =
                ((NamespaceDeclarationSyntax)classDeclarationSyntax.Parent).Name
                .ToString();

            var className = classDeclarationSyntax.Identifier.Text;

            return new MethodModel(
                methodName,
                className,
                parameters,
                usings,
                classNamespace);
        }

        private static ArgumentModel GetArgumentModel(ParameterSyntax p)
        {
            var name = p.Identifier.Text;
            var type = p.Type.ToString();
            var isFlag = type.EndsWith("bool") || type.EndsWith("Boolean");

            return new ArgumentModel(name, type, isFlag);
        }

        private static void AddAttribute(GeneratorExecutionContext context)
        {
            var code = @"
namespace Richiban.Cmdr.Generator
{
    public class CmdrMethodAttribute : System.Attribute {}
}";

            context.AddSource(
                "CmdrMethodAttribute.g.cs",
                SourceText.From(code, Encoding.UTF8));
        }

        private static void AddMainMethod(
            GeneratorExecutionContext context,
            ImmutableArray<MethodModel> methods)
        {
            var commands = methods.Select(MethodModelToString);

            var commandsString = string.Join("\n", commands);

            var usings = new UsingsModel
            {
                "System", "System.CommandLine", "System.CommandLine.Invocation", "Richiban.Cmdr"
            };

            usings.AddRange(methods.SelectMany(m => m.Usings));

            var usingsString = string.Join("\n", usings.Select(u => $"using {u};"));

            var code = @$"
{usingsString}

public static class Program 
{{
    public static int Main(string[] args)
    {{
        var rootCommand = new RootCommand();

        {commandsString}

        if (args is {{ Length: 1 }} && args[0] is ""--interactive"" or ""-i"")
        {{
            var repl = new Repl(rootCommand, ""What would you like to do?"");
            repl.EnterLoop();

            return 0;
        }}
        else
        {{
            return rootCommand.Invoke(args);
        }}
    }}
}}";

            //Debugger.Launch();

            context.AddSource("Program.g.cs", SourceText.From(code, Encoding.UTF8));
            context.AddSource("Repl.g.cs", SourceText.From(GetReplClass(), Encoding.UTF8));
        }

        private static string MethodModelToString(MethodModel method)
        {
            var parameterStrings = method.Arguments.Select(ArgumentOrOptionToString);

            var parametersString = string.Join(",\n", parameterStrings);

            var argumentTypes = method.Arguments.Select(a => a.Type);

            var argumentTypesString = string.Join(", ", argumentTypes);

            var cmdName = method.NameIn + "Command";

            return $@"
var {cmdName} = new Command(""{method.NameOut}"")
{{ 
    {parametersString}
}};

{cmdName}.Handler = CommandHandler.Create<{argumentTypesString}>({method.ClassName}.{method.NameIn});

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
            var aliases = new[] { argumentModel.NameOut[0].ToString(), argumentModel.NameOut };
            var aliasesString = string.Join(", ", aliases.Select(a => $"\"{a}\""));

            return $@"new Option(new string[] {{{aliasesString}}})";
        }

        private static string GetReplClass()
        {
            return @"using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace Richiban.Cmdr
{
    internal class Repl
    {
        private const int IndentationSize = 4;

        private readonly Dictionary<char, (string helpText, Action<Repl> action)>
            _commands = new();

        private readonly int _level;

        private readonly string _promptText;
        private bool _isEnd;

        public Repl(Command command, string promptText, int level = 0)
        {
            _promptText = promptText;
            _level = level;

            var valueTuples = command.OfType<Command>()
                .ToDictionary(
                    c => c.Name[index: 0],
                    c =>
                    {
                        return (helpText: c.Name, action: (Action<Repl>)(r => Act(c, r)));
                    });

            if (command is RootCommand)
            {
                valueTuples.Add(key: '/', (helpText: ""Quit"", action: r => r.End()));
            }
            else
            {
                valueTuples.Add(key: '/', (helpText: ""Go back"", action: r => r.End()));
            }

            _commands = valueTuples;
        }

        private string Indentation => new(c: ' ', _level * IndentationSize);

        private void Act(Command command, Repl repl)
        {
            if (command.OfType<Command>().Any())
            {
                var subRepl = new Repl(command, command.Name, _level + 1);
                subRepl.EnterLoop();
            }
            else
            {
                var arguments = command.OfType<Argument>()
                    .Select(
                        a =>
                        {
                            Write($""{a.Name}: "");

                            return PromptString(inline: false);
                        });

                var options = command.OfType<Option>()
                    .SelectMany(
                        o =>
                        {
                            Write($""{o.Name} [y/*]: "");

                            var promptChar = PromptChar();

                            Console.WriteLine();

                            switch (promptChar)
                            {
                                case 'y': return new[] { o.Aliases.First() };
                                case var _: return Array.Empty<string>();
                            }
                        })
                    .ToArray();

                command.Invoke(arguments.Concat(options).ToArray());
            }
        }

        public void End()
        {
            _isEnd = true;
        }

        public void Write(object toWrite) => Console.Write($""{Indentation}{toWrite}"");

        public void WriteLine(object toWrite) =>
            Console.WriteLine($""{Indentation}{toWrite}"");

        public void EnterLoop()
        {
            while (_isEnd == false)
            {
                WriteLine(""-------------------"");

                if (_promptText != null)
                {
                    WriteLine(_promptText);
                }

                foreach (var (triggerKey, (helpText, action)) in _commands)
                {
                    WriteLine($""[{triggerKey}] {helpText}"");
                }

                Console.WriteLine();

                var input = PromptChar(_commands.Keys.ToHashSet());

                Console.WriteLine();

                _commands[input].action(this);
            }
        }

        private string PromptString(bool inline = false)
        {
            List<char> result = new();

            ConsoleKeyInfo lastInput;

            do
            {
                lastInput = Console.ReadKey(intercept: true);

                if (lastInput.KeyChar is >= 'a' and <= 'z')
                {
                    result.Add(lastInput.KeyChar);
                    Console.Write(lastInput.KeyChar);
                }
            } while (lastInput.Key != ConsoleKey.Enter &&
                     lastInput.Key != ConsoleKey.Tab);

            if (inline == false && lastInput.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
            }

            return new string(result.ToArray());
        }

        private char PromptChar(ISet<char> allowedCharacters = null)
        {
            allowedCharacters ??= ""abcdefghijklmnopqrstuvwxyz"".ToHashSet();

            Write(""[ ]"");

            Console.SetCursorPosition(Console.CursorLeft - 2, Console.CursorTop);

            char input;

            do
            {
                input = Console.ReadKey(intercept: true).KeyChar;
            } while (!allowedCharacters.Contains(input));

            Console.Write(input);

            return input;
        }
    }
}";
        }
    }
}