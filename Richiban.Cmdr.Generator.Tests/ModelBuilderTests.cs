using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using NUnit.Framework;

namespace Richiban.Cmdr.Generator.Tests
{
    [TestFixture]
    class ModelBuilderTests
    {
        [Test]
        public void SimplestGeneratorTest()
        {
            var source = @"";
            var (outputCompilation, diagnostics) = RunGenerator(source);

            Assert.That(diagnostics, Is.Empty);

            Assert.That(
                outputCompilation.SyntaxTrees.Count,
                Is.EqualTo(4),
                "We expected four syntax trees: the original one plus the four we generated");

            var programFile = outputCompilation.SyntaxTrees.Single(
                s => s.FilePath.EndsWith("Program.g.cs"));

            var cmdrAttribute = outputCompilation.SyntaxTrees.Single(
                s => s.FilePath.EndsWith("CmdrMethodAttribute.g.cs"));

            var repl = outputCompilation.SyntaxTrees.Single(
                s => s.FilePath.EndsWith("Repl.g.cs"));

            var original = outputCompilation.SyntaxTrees.Single(s => s.FilePath == "");

            Console.WriteLine(programFile.GetText().ToString());
        }

        [Test]
        public void InstanceMethodGivesDiagnosticError()
        {
            var source = @"
namespace Richiban.Cmdr
{
    public class CmdrMethodAttribute : System.Attribute
    {
        public CmdrMethodAttribute(params string[] names)
        {
            Names = names;
        }

        public string[] Names { get; }
    }
}

namespace TestSamples
{
    public class TestClass
    {
        [CmdrMethod]
        public void TestMethod()
        {
        }
    }
}
";
            var (_, diagnostics) = RunGenerator(source);

            Assert.That(diagnostics, Has.Length.EqualTo(1));
            var diagnostic = diagnostics.Single();
            Assert.That(diagnostic.Severity, Is.EqualTo(DiagnosticSeverity.Error));
            Assert.That(diagnostic.Id, Is.EqualTo("Cmdr0001"));
            Assert.That(diagnostic.GetMessage(), Is.EqualTo("Method TestSamples.TestClass.TestMethod() must be static in order to use the CmdrMethod attribute."));
        }

        private static (Compilation, ImmutableArray<Diagnostic>) RunGenerator(
            string source)
        {
            var inputCompilation = CSharpCompilation.Create(
                "compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[]
                {
                    MetadataReference.CreateFromFile(
                        typeof(Binder).GetTypeInfo().Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            var generator = new CmdrGenerator();

            var driver = CSharpGeneratorDriver.Create(generator)
                .RunGeneratorsAndUpdateCompilation(
                    inputCompilation,
                    out var outputCompilation,
                    out var diagnostics);

            return (outputCompilation, diagnostics);
        }
    }
}