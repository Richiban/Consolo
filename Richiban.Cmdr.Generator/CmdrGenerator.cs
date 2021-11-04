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
                "System", "System.CommandLine", "System.CommandLine.Invocation"
            };

            usings.AddRange(methods.SelectMany(m => m.Usings));

            var usingsString = string.Join("\n", usings.Select(u => $"using {u};"));

            var title = "Welcome to your app";

            var code = @$"
{usingsString}

public static class Program 
{{
    public static void Main(string[] args)
    {{
        System.Console.WriteLine(""{title}"");

var rootCommand = new RootCommand();

        {commandsString}

rootCommand.Invoke(args);
    }}
}}";

            context.AddSource("Program.g.cs", SourceText.From(code, Encoding.UTF8));
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

            return $@"new Option({aliasesString})";
        }
    }
}