using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Consolo.Tests.Generator;

internal abstract class GeneratorTests
{
    protected static SyntaxTree GetConsoloAttributeFile(Compilation outputCompilation)
    {
        return outputCompilation.SyntaxTrees.Single(
            s => s.FilePath.EndsWith("ConsoloAttribute.g.cs"));
    }

    protected static SyntaxTree GetProgramSyntaxTree(Compilation outputCompilation)
    {
        return outputCompilation.SyntaxTrees.Single(
            x => x.FilePath.EndsWith("Program.g.cs"));
    }

    protected static (Compilation, ImmutableArray<Diagnostic>) RunGenerator(
        string source)
    {
        var inputCompilation = CSharpCompilation.Create(
            "compilation",
            [CSharpSyntaxTree.ParseText(source)],
            [
                MetadataReference.CreateFromFile(
                        typeof(Binder).GetTypeInfo().Assembly.Location)
            ],
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        var generator = new ConsoloSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(
                inputCompilation,
                out var outputCompilation,
                out var diagnostics);

        return (outputCompilation, diagnostics);
    }
}

