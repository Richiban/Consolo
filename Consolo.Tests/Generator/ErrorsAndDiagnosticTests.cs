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
            () => error.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(7)
        );
    }

    [Test]
    public void MultiCharacterAliasResultsInError()
    {
        var source =
            """
            using Consolo;

            namespace TestSamples;
            
            public static class TestClass
            {
                [Consolo]
                public static void TestMethod([Consolo(Alias = "ab")] string a = null)
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

        var error = errors.ShouldHaveSingleItem();

        error.ShouldSatisfyAllConditions(
            () => error.Id.ShouldBe("Consolo0009"),
            () => error.Severity.ShouldBe(DiagnosticSeverity.Error),
            () => error.GetMessage().ShouldBe("A parameter's alias must be exactly one character."),
            () => error.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(7)
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
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

        var error = errors.ShouldHaveSingleItem();

        error.ShouldSatisfyAllConditions(
            () => error.Id.ShouldBe("Consolo0013"),
            () => error.Severity.ShouldBe(DiagnosticSeverity.Error),
            () => error.GetMessage().ShouldBe("The parameter name or alias 'a' is already in use in command 'TestMethod'."),
            () => error.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(9)
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
                    [Consolo("p1")] string a = null, 
                    [Consolo("p1")] string b = null)
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

        var error = errors.ShouldHaveSingleItem();

        error.ShouldSatisfyAllConditions(
            () => error.Id.ShouldBe("Consolo0013"),
            () => error.Severity.ShouldBe(DiagnosticSeverity.Error),
            () => error.GetMessage().ShouldBe("The parameter name or alias 'p1' is already in use in command 'TestMethod'."),
            () => error.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(9)
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
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

        var error = errors.ShouldHaveSingleItem();

        error.ShouldSatisfyAllConditions(
            () => error.Id.ShouldBe("Consolo0008"),
            () => error.Severity.ShouldBe(DiagnosticSeverity.Error),
            () => error.GetMessage().ShouldBe("Parameter 'x' has a type that is unsupported (Foo)."),
            () => error.Location.GetLineSpan().StartLinePosition.Line.ShouldBe(10)
        );
    }
}

