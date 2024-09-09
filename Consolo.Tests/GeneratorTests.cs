using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq;
using Shouldly;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Consolo.Tests;

[TestFixture]
internal class GeneratorTests
{
    [Test]
    public void ConsoloAttributeFileTest()
    {
        var source =
            """
                using Consolo;

                public static class TestClass
                {
                    [Consolo]
                    public static void TestMethod()
                    {
                    }
                }
                """;

        var (outputCompilation, diagnostics) = RunGenerator(source);

        Assert.That(
            diagnostics
                .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning), 
            Is.Empty
        );

        {
            var fileNames = string.Join(
                ",\n",
                outputCompilation.SyntaxTrees.Select(s => s.FilePath));

            Assert.That(
                outputCompilation.SyntaxTrees.Count,
                Is.EqualTo(expected: 3),
                $"We expected four syntax trees: the original one plus the three we generated. Found: {fileNames}");
        }

        var ConsoloAttributeFile = GetConsoloAttributeFile(outputCompilation);

        var src = ConsoloAttributeFile.GetText().ToString();

        src.ShouldBe(
            """
            using System;

            namespace Consolo
            {
                [AttributeUsage(
                    AttributeTargets.Method 
                    | AttributeTargets.Class
                    | AttributeTargets.Parameter,
                    Inherited = false,
                    AllowMultiple = false)]
                internal class ConsoloAttribute : Attribute
                {
                    public ConsoloAttribute(string name = null)
                    {
                    }

                    public string Alias { get; set; }
                }
            }
            """);
    }

    [Test]
    public void InstanceMethodGivesDiagnosticError()
    {
        var source = 
            """
            using Consolo;

            namespace TestSamples
            {
                public class TestClass
                {
                    [Consolo]
                    public void TestMethod()
                    {
                    }
                }
            }
            """;

        var (_, diagnostics) = RunGenerator(source);
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

        var error = errors.ShouldHaveSingleItem();
        Assert.That(error.Severity, Is.EqualTo(DiagnosticSeverity.Error));
        Assert.That(error.Id, Is.EqualTo("Consolo0005"));

        Assert.That(
            error.GetMessage(),
            Is.EqualTo(
                "Method TestSamples.TestClass.TestMethod() must be static in order to use the Consolo attribute."));
    }

    [Test]
    public void StaticMethodWithAutoName()
    {
        var source = 
        """
        using Consolo;

        namespace TestSamples;

        public class TestClass
        {
            [Consolo]
            public static void TestMethod()
            {
            }
        }
        """;

        var (compilation, diagnostics) = RunGenerator(source);

        Assert.That(diagnostics, Is.Empty);

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldBe(
            @"using System;
using System.CommandLine;
using System.CommandLine.Invocation;


public static class Program
{
    public static int Main(string[] args)
    {
        var testMethodCommand = new Command(""test-method"")
        {
        };

        testMethodCommand.Handler = CommandHandler.Create(TestSamples.TestClass.TestMethod);

        var rootCommand = new RootCommand()
        {
            testMethodCommand
        };

        if (Repl.IsCall(args))
        {
            Repl.EnterNewLoop(rootCommand, ""Select a command"");

            return 0;
        }
        else
        {
            return rootCommand.Invoke(args);
        }
    }
}
");
    }

    [Test]
    public void ExplicitNameChangesCommandName()
    {
        var source = @"

using System;

namespace TestSamples
{
    public class TestClass
    {
        [Consolo(""explicit"")]
        public static void TestMethod()
        {
        }
    }
}
";

        var (compilation, diagnostics) = RunGenerator(source);

        Assert.That(diagnostics, Is.Empty);

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldBe(
            @"using System;
using System.CommandLine;
using System.CommandLine.Invocation;


public static class Program
{
    public static int Main(string[] args)
    {
        var testMethodCommand = new Command(""explicit"")
        {
        };

        testMethodCommand.Handler = CommandHandler.Create(TestSamples.TestClass.TestMethod);

        var rootCommand = new RootCommand()
        {
            testMethodCommand
        };

        if (Repl.IsCall(args))
        {
            Repl.EnterNewLoop(rootCommand, ""Select a command"");

            return 0;
        }
        else
        {
            return rootCommand.Invoke(args);
        }
    }
}
");
    }

    [Test]
    public void NestedAttributeUsage()
    {
        var source = @"
namespace TestSamples
{
    [Consolo]
    public class OuterTest
    {
        [Consolo]
        public class InnerTest1
        {        
            [Consolo]
            public static void TestMethod1()
            {
            } 

            [Consolo]
            public static void TestMethod2(string arg1, string arg2)
            {
            }
        }

        [Consolo]
        public class InnerTest2
        {
            [Consolo]
            public static void TestMethod3()
            {
            }
        }
    }
}
";

        var (outputCompilation, diagnostics) = RunGenerator(source);

        Assert.That(diagnostics, Is.Empty);

        var programSource =
            GetProgramSyntaxTree(outputCompilation).GetText().ToString();

        programSource.ShouldBe(
            @"using System;
using System.CommandLine;
using System.CommandLine.Invocation;


public static class Program
{
    public static int Main(string[] args)
    {
        var testMethod1Command = new Command(""test-method1"")
        {
        };

        testMethod1Command.Handler = CommandHandler.Create(TestSamples.OuterTest.InnerTest1.TestMethod1);

        var testMethod2Command = new Command(""test-method2"")
        {
            new Argument(""arg1"")
            ,
            new Argument(""arg2"")
        };

        testMethod2Command.Handler = CommandHandler.Create<System.String, System.String>(TestSamples.OuterTest.InnerTest1.TestMethod2);

        var testMethod3Command = new Command(""test-method3"")
        {
        };

        testMethod3Command.Handler = CommandHandler.Create(TestSamples.OuterTest.InnerTest2.TestMethod3);

        var rootCommand = new RootCommand()
        {
            new Command(""outer-test"")
            {
                new Command(""inner-test1"")
                {
                    testMethod1Command
                    ,
                    testMethod2Command
                }
                ,
                new Command(""inner-test2"")
                {
                    testMethod3Command
                }
            }
        };

        if (Repl.IsCall(args))
        {
            repl.EnterNewLoop(rootCommand, ""Select a command"");

            return 0;
        }
        else
        {
            return rootCommand.Invoke(args);
        }
    }
}
");
    }

    [Test]
    public void EmptyCommandNameResultsInShortenedPath()
    {
        var source = 
            """
            using Consolo;

            namespace TestSamples
            {
                [Consolo("aaa")
                public class TestClass
                {
                    [Consolo("")]
                    public static void TestMethodA()
                    {
                    }

                    [Consolo("bbb")]
                    public static void TestMethodB()
                    {
                    }
                }
            }
            """;

        var (compilation, diagnostics) = RunGenerator(source);

        diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ShouldBeEmpty();

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldBe(
            """
            using System;

            public static class Program
            {
                public static int Main(string[] args)
                {
                    var testMethodBCommand = new Command(""bbb"")
                    {
                    };

                    testMethodBCommand.Handler = CommandHandler.Create(TestSamples.TestClass.TestMethodB);

                    var testMethodACommand = new Command(""aaa"")
                    {
                        testMethodBCommand
                    };

                    testMethodACommand.Handler = CommandHandler.Create(TestSamples.TestClass.TestMethodA);

                    var rootCommand = new RootCommand()
                    {
                        testMethodACommand
                    };

                    if (Repl.IsCall(args))
                    {
                        Repl.EnterNewLoop(rootCommand, ""Select a command"");

                        return 0;
                    }
                    else
                    {
                        return rootCommand.Invoke(args);
                    }
                }
            }
            """
        );
    }

    [Test]
    public void RootWithCommandHandlerTest()
    {
        var source = @"
using System;

namespace TestSamples
{
    public class RootContainer
    {
        [Consolo("""")]
        public static void RootMethod()
        {
        }
    }
}";

        var (compilation, diagnostics) = RunGenerator(source);

        Assert.That(diagnostics, Is.Empty);

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldBe(
            @"using System;
using System.CommandLine;
using System.CommandLine.Invocation;


public static class Program
{
    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand()
        {
        };

        rootCommand.Handler = CommandHandler.Create(TestSamples.RootContainer.RootMethod);

        if (Repl.IsCall(args))
        {
            Repl.EnterNewLoop(rootCommand, ""Select a command"");

            return 0;
        }
        else
        {
            return rootCommand.Invoke(args);
        }
    }
}
");
    }

    [Test]
    public void LargeExampleInvolvingNestedAttributesEmptyAttributesAndRootCommand()
    {
        var source = @"using System;

namespace Consolo.Samples
{
    [Consolo(""remote"")]
    public class SampleProgram
    {
        [Consolo("""")]
        public static void ListRemotes()
        {
            Console.WriteLine(""Listing remotes"");
        }
            
        [Consolo(""remove"")]
        public static void RemoveRemote(string remoteName)
        {
            Console.WriteLine(
                $""Removing remote '{remoteName}'"");
        }
        
        [Consolo(""add"")]
        public static void AddRemote(string remoteName, string url)
        {
            Console.WriteLine(
                $""Adding remote '{remoteName}' with url '{url}'"");
        }
    }
    
    [Consolo(""branch"")]
    public class BranchActions
    {
        [Consolo("""")]
        public static void ListBranches()
        {
            Console.WriteLine(""Listing branches"");
        }
        
        [Consolo("""")]
        public static void CreateBranches(string branchName)
        {
            Console.WriteLine($""Creating branch {branchName}"");
        }
    }
    
    public class Root
    {
        [Consolo("""")]
        public static void MainRoot()
        {
            Console.WriteLine(""In the root command"");
        }
    }
}";

        var (compilation, diagnostics) = RunGenerator(source);

        Assert.That(diagnostics, Is.Empty);

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldBe(
            @"using System;
using System.CommandLine;
using System.CommandLine.Invocation;


public static class Program
{
    public static int Main(string[] args)
    {
        var createBranchesCommand = new Command(""branch"")
        {
            new Argument(""branchName"")
        };

        createBranchesCommand.Handler = CommandHandler.Create<System.String>(Consolo.Samples.BranchActions.CreateBranches);

        var addRemoteCommand = new Command(""add"")
        {
            new Argument(""remoteName"")
            ,
            new Argument(""url"")
        };

        addRemoteCommand.Handler = CommandHandler.Create<System.String, System.String>(Consolo.Samples.SampleProgram.AddRemote);

        var removeRemoteCommand = new Command(""remove"")
        {
            new Argument(""remoteName"")
        };

        removeRemoteCommand.Handler = CommandHandler.Create<System.String>(Consolo.Samples.SampleProgram.RemoveRemote);

        var listRemotesCommand = new Command(""remote"")
        {
            removeRemoteCommand
            ,
            addRemoteCommand
        };

        listRemotesCommand.Handler = CommandHandler.Create(Consolo.Samples.SampleProgram.ListRemotes);

        var rootCommand = new RootCommand()
        {
            listRemotesCommand
            ,
            createBranchesCommand
        };

        rootCommand.Handler = CommandHandler.Create(Consolo.Samples.Root.MainRoot);

        if (Repl.IsCall(args))
        {
            Repl.EnterNewLoop(rootCommand, ""Select a command"");

            return 0;
        }
        else
        {
            return rootCommand.Invoke(args);
        }
    }
}");
    }

    private static SyntaxTree GetOriginalSourceFile(Compilation outputCompilation)
    {
        return outputCompilation.SyntaxTrees.Single(s => s.FilePath == "");
    }

    private static SyntaxTree GetReplFile(Compilation outputCompilation)
    {
        return outputCompilation.SyntaxTrees.Single(
            s => s.FilePath.EndsWith("Repl.g.cs"));
    }

    private static SyntaxTree GetConsoloAttributeFile(Compilation outputCompilation)
    {
        return outputCompilation.SyntaxTrees.Single(
            s => s.FilePath.EndsWith("ConsoloAttribute.g.cs"));
    }

    private static SyntaxTree GetProgramSyntaxTree(Compilation outputCompilation)
    {
        return outputCompilation.SyntaxTrees.Single(
            x => x.FilePath.EndsWith("Program.g.cs"));
    }

    private static (Compilation, ImmutableArray<Diagnostic>) RunGenerator(
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
