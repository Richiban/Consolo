using System;
using NUnit.Framework;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Writers;
using Shouldly;

namespace Richiban.Cmdr.Tests
{
    [TestFixture]
    public class ProgramClassWriterTests
    {
        [Test]
        public void BasicTest()
        {
            var methods = new[]
            {
                new MethodModel(
                    "SomeFunction",
                    providedName: null,
                    Array.Empty<string>(),
                    "SomeClass",
                    Array.Empty<ArgumentModel>())
            };

            var codeGenerator = new ProgramClassFileGenerator(methods);

            var actual = codeGenerator.GetCode();

            actual.ShouldBe(@"using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Richiban.Cmdr;

public static class Program
{
    public static int Main(string[] args)
    {
        var someFunctionCommand = new Command(""some-function"")
        {
        };

        someFunctionCommand.Handler = CommandHandler.Create(SomeClass.SomeFunction);

        var rootCommand = new RootCommand()
        {
            someFunctionCommand
        };

        if (args.Length == 1 && (args[0] == ""--interactive"" || args[0] == ""-i""))
        {
            var repl = new Repl(rootCommand, ""Select a command"");
            repl.EnterLoop();

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
    }
}