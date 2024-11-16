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
        string source, string assemblyName = "Consolo.TestSource")
    {
        var inputCompilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            [
                MetadataReference.CreateFromFile(
                        typeof(Binder).GetTypeInfo().Assembly.Location),
                ..(GetAssemblies())
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

    private static IEnumerable<PortableExecutableReference> GetAssemblies()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!String.IsNullOrEmpty(assembly.Location))
                yield return MetadataReference.CreateFromFile(assembly.Location);
        }
    }
}

