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
            // Get all classes
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

            if (attributes.Any())
            {
                var methodName = method.Identifier.Text;

                var componentName = Utils.ToKebabCase(methodName);

                return new MethodModel(componentName);
            }

            return null;
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
            ImmutableArray<MethodModel> components)
        {
            var lines = components.Select(
                it => $@"rootCommand.Add(new Command(""{it.MethodName}""));");

            var code = @$"
using System.CommandLine;

public static class Program 
{{
    public static void Main(string[] args)
    {{
        System.Console.WriteLine(""In the auto main method"");

var rootCommand = new RootCommand();

        {string.Join("\n", lines)}

rootCommand.Invoke(args);
    }}
}}";

            context.AddSource("Program.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }
}