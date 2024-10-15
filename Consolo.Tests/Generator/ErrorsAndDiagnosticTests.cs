using Microsoft.CodeAnalysis;

namespace Consolo.Tests.Generator;

internal class ErrorsAndDiagnosticsTests : GeneratorTests
{
    [Test]
    public void NoCommandsResultsInError()
    {
        var source =
            """
            using Consolo;

            namespace TestSamples;
            
            public class TestClass
            {
                public void TestMethod()
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

        var error = errors.ShouldHaveSingleItem();

        error.ShouldSatisfyAllConditions(
            () => error.Id.ShouldBe("Consolo0002"),
            () => error.Severity.ShouldBe(DiagnosticSeverity.Error),
            () => error.GetMessage().ShouldBe("No command methods found. Make sure you're using the Consolo attribute."),
            () => error.Location.ShouldBe(Location.None)
        );
    }

    [Test]
    public void InstanceMethodGivesDiagnosticError()
    {
        var source =
            """
            using Consolo;

            namespace TestSamples;
            
            public class TestClass
            {
                [Consolo]
                public void TestMethod()
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

        var error = errors.ShouldHaveSingleItem();

        error.ShouldSatisfyAllConditions(
            () => error.Id.ShouldBe("Consolo0005"),
            () => error.Severity.ShouldBe(DiagnosticSeverity.Error),
            () => error.GetMessage().ShouldBe("Method TestSamples.TestClass.TestMethod() must be static in order to use the Consolo attribute.")
        );
    }

    [Test]
    public void PositionalParamWithAliasResultsInError()
    {
        var source =
            """
            using Consolo;

            namespace TestSamples;
            
            public static class TestClass
            {
                [Consolo]
                public static void TestMethod([Consolo(Alias = "a")] string a)
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

        var error = errors.ShouldHaveSingleItem();

        error.ShouldSatisfyAllConditions(
            () => error.Id.ShouldBe("Consolo0010"),
            () => error.Severity.ShouldBe(DiagnosticSeverity.Error),
            () => error.GetMessage().ShouldBe("Positional parameters cannot have an alias."),
            () => error.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(7),
            () => error.Location.GetLineSpan().StartLinePosition.Character.ShouldBe(43),
            () => error.Location.SourceTree.GetText().ToString(error.Location.SourceSpan).ShouldBe("Alias = \"a\"")
        );
    }

    [TestCase("/")]
    [TestCase("-a")]
    [TestCase("")]
    public void IllegalParameterNameResultsInError(string paramName)
    {
        var source =
            $$"""
            using Consolo;

            namespace TestSamples;
            
            public static class TestClass
            {
                [Consolo]
                public static void TestMethod([Consolo("{{paramName}}")] string a)
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

        var error = errors.ShouldHaveSingleItem();

        error.ShouldSatisfyAllConditions(
            () => error.Id.ShouldBe("Consolo0007"),
            () => error.Severity.ShouldBe(DiagnosticSeverity.Error),
            () => error.GetMessage().ShouldBe("Parameter names must have the form [a-z][-a-zA-Z]*."),
            () => error.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(7),
            () => error.Location.GetLineSpan().StartLinePosition.Character.ShouldBe(43),
            () => error.Location.SourceTree.GetText().ToString(error.Location.SourceSpan).ShouldBe($"\"{paramName}\"")
        );
    }

    [TestCase("")]
    [TestCase("-")]
    [TestCase("/")]
    [TestCase("ab")]
    public void IllegalAliasResultsInError(string aliasName)
    {
        var source =
            $$"""
            using Consolo;

            namespace TestSamples;
            
            public static class TestClass
            {
                [Consolo]
                public static void TestMethod([Consolo(Alias = "{{aliasName}}")] string a = null)
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);

        diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                error => error.Id.ShouldBe("Consolo0009"),
                error => error.Severity.ShouldBe(DiagnosticSeverity.Error),
                error => error.GetMessage().ShouldBe("Parameter aliases must be a single character in the range [a-z]."),
                error => error.Location.ShouldSatisfyAllConditions(
                    loc => loc.GetLineSpan().StartLinePosition.Line.ShouldBe(7),
                    loc => loc.GetLineSpan().StartLinePosition.Character.ShouldBe(43),
                    loc => loc.SourceTree.GetText().ToString(error.Location.SourceSpan).ShouldBe($"Alias = \"{aliasName}\"")
                )
            );
    }

    [Test]
    public void TwoParamsWithSameAliasResultsInError()
    {
        var source =
            """
            using Consolo;

            namespace TestSamples;
            
            public static class TestClass
            {
                [Consolo]
                public static void TestMethod(
                    [Consolo(Alias = "a")] string a = null, 
                    [Consolo(Alias = "a")] string b = null)
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);

        diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                error => error.Id.ShouldBe("Consolo0013"),
                error => error.Severity.ShouldBe(DiagnosticSeverity.Error),
                error => error.GetMessage().ShouldBe("The parameter name or alias 'a' is already in use in command 'TestMethod'."),
                error => error.Location.ShouldSatisfyAllConditions(
                    loc => loc.GetLineSpan().StartLinePosition.Line.ShouldBe(9),
                    loc => loc.GetLineSpan().StartLinePosition.Character.ShouldBe(17),
                    loc => loc.SourceTree.GetText().ToString(loc.SourceSpan).ShouldBe("Alias = \"a\"")
                )
            );
    }

    [Test]
    public void TwoParamsWithSameNameResultsInError()
    {
        var source =
            """
            using Consolo;

            namespace TestSamples;
            
            public static class TestClass
            {
                [Consolo]
                public static void TestMethod(
                    [Consolo("p1")] 
                    string a = null, 
                    [Consolo("p1")]
                    string b = null)
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);

        diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                error => error.Id.ShouldBe("Consolo0013"),
                error => error.Severity.ShouldBe(DiagnosticSeverity.Error),
                error => error.GetMessage().ShouldBe("The parameter name or alias 'p1' is already in use in command 'TestMethod'."),
                error => error.Location.ShouldSatisfyAllConditions(
                    loc => loc.GetLineSpan().StartLinePosition.Line.ShouldBe(10),
                    loc => loc.GetLineSpan().StartLinePosition.Character.ShouldBe(17),
                    loc => loc.SourceTree.GetText().ToString(loc.SourceSpan).ShouldBe("\"p1\"")
                )
            );
    }

    [Test]
    public void ParameterTypeUnsupportedResultsInError()
    {
        var source =
            """
            using Consolo;

            namespace TestSamples;

            public class Foo {}
            
            public static class TestClass
            {
                [Consolo]
                public static void TestMethod(
                    Foo x)
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);

        diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                error => error.Id.ShouldBe("Consolo0008"),
                error => error.Severity.ShouldBe(DiagnosticSeverity.Error),
                error => error.GetMessage().ShouldBe("Parameter 'x' has a type that is unsupported (Foo)."),
                error => error.Location.ShouldSatisfyAllConditions(
                    loc => loc.GetLineSpan().StartLinePosition.Line.ShouldBe(10),
                    loc => loc.GetLineSpan().StartLinePosition.Character.ShouldBe(12),
                    loc => loc.SourceTree.GetText().ToString(loc.SourceSpan).ShouldBe("x")
                )
            );
    }
}

