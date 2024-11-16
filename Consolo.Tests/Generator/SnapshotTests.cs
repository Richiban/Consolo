using Microsoft.CodeAnalysis;

namespace Consolo.Tests.Generator;

internal class SnapshotTests : GeneratorTests
{
    [Test]
    public void ConsoloAttributeFileTest()
    {
        var source =
            """
            using Consolo;

            namespace TestProgram;

            public static class TestClass
            {
                [Consolo]
                public static void TestMethod()
                {
                }
            }
            """;

        var (outputCompilation, diagnostics) = RunGenerator(source);

        diagnostics.WarningsAndErrors().ShouldBeEmpty();

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

        src.ShouldMatchSnapshot("ConsoloAttribute.g");
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

        diagnostics.WarningsAndErrors().ShouldBeEmpty();

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldMatchSnapshot();
    }

    [Test]
    public void ExplicitNameChangesCommandName()
    {
        var source =
            """
            using System;
            using Consolo;

            namespace TestSamples
            {
                public class TestClass
                {
                    [Consolo("explicit")]
                    public static void TestMethod()
                    {
                    }
                }
            }
            """;

        var (compilation, diagnostics) = RunGenerator(source);

        diagnostics.WarningsAndErrors().ShouldBeEmpty();

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldMatchSnapshot();
    }

    [Test]
    public void NestedAttributeUsage()
    {
        var source =
            """
            using Consolo;
            namespace TestSamples;
        
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
            """;

        var (outputCompilation, diagnostics) = RunGenerator(source);

        diagnostics.WarningsAndErrors().ShouldBeEmpty();

        var programSource =
            GetProgramSyntaxTree(outputCompilation).GetText().ToString();

        programSource.ShouldMatchSnapshot();
    }

    [Test]
    public void EmptyCommandNameResultsInShortenedPath()
    {
        var source =
            """
            using Consolo;

            namespace TestSamples;
            
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
            """;

        var (compilation, diagnostics) = RunGenerator(source);

        diagnostics.WarningsAndErrors().ShouldBeEmpty();

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldMatchSnapshot();
    }

    [Test]
    public void RootWithCommandHandlerTest()
    {
        var source =
            """
            using System;
            using Consolo;

            namespace TestSamples;
            
            public class RootContainer
            {
                [Consolo("")]
                public static void RootMethod()
                {
                }
            }
            """;

        var (compilation, diagnostics) = RunGenerator(source);

        diagnostics.WarningsAndErrors().ShouldBeEmpty();

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldMatchSnapshot();
    }

    [Test]
    public void LargeExampleInvolvingNestedAttributesEmptyAttributesAndRootCommand()
    {
        var source = """
            using System;
            using Consolo;

            namespace Consolo.Samples;
        
            [Consolo("remote")]
            public class SampleProgram
            {
                [Consolo("")]
                public static void ListRemotes()
                {
                    Console.WriteLine(""Listing remotes"");
                }
                    
                [Consolo("remove")]
                public static void RemoveRemote(string remoteName)
                {
                    Console.WriteLine(
                        $"Removing remote '{remoteName}'");
                }
                
                [Consolo("add")]
                public static void AddRemote(string remoteName, string url)
                {
                    Console.WriteLine(
                        $"Adding remote '{remoteName}' with url '{url}'");
                }
            }
            
            [Consolo("branch")]
            public class BranchActions
            {
                [Consolo("")]
                public static void ListBranches()
                {
                    Console.WriteLine("Listing branches");
                }
            }
            
            public class Root
            {
                [Consolo("")]
                public static void MainRoot()
                {
                    Console.WriteLine("In the root command");
                }
            }
            """;

        var (compilation, diagnostics) = RunGenerator(source);

        diagnostics.WarningsAndErrors().ShouldBeEmpty();

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldMatchSnapshot();
    }

    [Test]
    public void ParamDefaultValueIsRespected()
    {
        var source = """
                     using System;
                     using Consolo;

                     namespace Consolo.Samples;

                     public class SampleProgram
                     {
                         [Consolo]
                         public static void DefaultValueTest(TestEnum arg = TestEnum.B)
                         {
                             Console.WriteLine($""arg = {arg}"");
                         }
                     }
                     
                     public enum TestEnum
                     {
                         A, B
                     }
                     """;

        var (compilation, diagnostics) = RunGenerator(source);

        diagnostics.WarningsAndErrors().ShouldBeEmpty();

        var programText = GetProgramSyntaxTree(compilation).GetText().ToString();

        programText.ShouldMatchSnapshot();
    }
}

