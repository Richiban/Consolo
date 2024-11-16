using System.Text;
using System.IO;
using Consolo.Samples.g;

namespace Consolo.Tests.E2E;

[TestFixture]
internal class E2ETests
{
    private readonly StringBuilder _testError = new();
    private readonly StringBuilder _testOutput = new();
    private static readonly TextWriter _defaultOut = Console.Out;
    private static readonly TextWriter _defaultError = Console.Error;

    [SetUp]
    public void SetUp()
    {
        Console.SetOut(new StringWriter(_testOutput));
        Console.SetError(new StringWriter(_testError));
    }

    [TearDown]
    public void TearDown()
    {
        _testOutput.Clear();
        _testError.Clear();

        Console.SetOut(_defaultOut);
        Console.SetError(_defaultError);
    }

    [Test]
    public void NoArgsResultsInHelpText()
    {
        Program.Main([]);

        _testOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void ExplicitHelpResultsInHelpText()
    {
        Program.Main(["--help"]);

        _testOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void LogCommandWithMissingArgResultsInErrorAndHelpText()
    {
        Program.Main(["log"]);

        _testError.ToString().ShouldBe($"Missing value for argument 'lines'{Environment.NewLine}");
        _testOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void LogCommandWithLinesArgumentRunsCommand()
    {
        Program.Main(["log", "1"]);

        _testError.ToString().ShouldBeEmpty();
        _testOutput.ToString().ShouldMatchSnapshot();
    }

    [TestCase([new[] { "--pretty", "oneline" }])]
    [TestCase([new[] { "--pretty=oneline" }])]
    [TestCase([new[] { "--pretty:oneline" }])]
    public void LogCommandWithPrettyArgumentRunsCommand(string[] args)
    {
        Program.Main(["log", "1", ..args]);

        _testError.ToString().ShouldBeEmpty();
        _testOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void LogCommandWithUnknownOptionResultsInErrorMessage()
    {
        Program.Main(["log", "--foo"]);

        _testError.ToString().ShouldBe($"Missing value for argument 'lines'{Environment.NewLine}Unrecognised args: --foo{Environment.NewLine}");
        _testOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void LogCommandWithUnparseableLinesValueResultsInException()
    {
        var ex = Assert.Throws<FormatException>(() => Program.Main(["log", "x"]));

        ex.Message.ShouldBe("The input string 'x' was not in a correct format.");
    }

    [Test]
    public void LogCommandWithUnparseablePrettyValueResultsInException()
    {
        var ex = Assert.Throws<ArgumentException>(() => Program.Main(["log", "1", "--pretty", "invalid"]));

        ex.Message.ShouldBe("Requested value 'invalid' was not found.");
    }

    [Test]
    public void VersionCommandPrintsVersion()
    {
        Program.Main(["--version"]);

        _testError.ToString().ShouldBeEmpty();
        _testOutput.ToString().ShouldMatchSnapshot();
    }

    [TestCase("name", "--loud")]
    [TestCase("--loud", "name")]
    [TestCase("name", "-l")]
    [TestCase("-l", "name")]
    public void GreetCommandIsAgnosticToArgumentOrder(string arg1, string arg2)
    {
        Program.Main(["greet", arg1, arg2]);

        _testError.ToString().ShouldBeEmpty();
        _testOutput.ToString().ShouldMatchSnapshot();
    }

    [TestCase("-?")]
    [TestCase("-h")]
    [TestCase("--help")]
    public void LogHelp(string helpArg)
    {
        Program.Main(["log", helpArg]);

        _testError.ToString().ShouldBeEmpty();
        _testOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void RemoteCommandParsesUrlCorrectly()
    {
        Program.Main(["remote", "add", "name", "http://example.com"]);

        _testError.ToString().ShouldBeEmpty();
        _testOutput.ToString().ShouldMatchSnapshot();
    }
}