using System.Text;
using System.IO;
using Consolo.Samples;

namespace Consolo.Tests.E2E;

[TestFixture]
internal class E2ETests
{
    private readonly StringBuilder _errorOutput = new();
    private readonly StringBuilder _standardOutput = new();
    private readonly TextWriter _a = Console.Out;
    private readonly TextWriter _b = Console.Error;

    [SetUp]
    public void SetUp()
    {
        Console.SetOut(new StringWriter(_standardOutput));
        Console.SetError(new StringWriter(_errorOutput));
    }

    [TearDown]
    public void TearDown()
    {
        _standardOutput.Clear();
        _errorOutput.Clear();

        Console.SetOut(_a);
        Console.SetError(_b);
    }

    [Test]
    public void NoArgsResultsInHelpText()
    {
        Program.Main([]);

        _standardOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void LogCommandWithMissingArgResultsInErrorAndHelpText()
    {
        Program.Main(["log"]);

        _errorOutput.ToString().ShouldBe($"Missing value for argument 'lines'{Environment.NewLine}");
        _standardOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void LogCommandWithLinesArgumentRunsCommand()
    {
        Program.Main(["log", "1"]);

        _errorOutput.ToString().ShouldBeEmpty();
        _standardOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void LogCommandWithPrettyArgumentRunsCommand()
    {
        Program.Main(["log", "1", "--pretty", "oneline"]);

        _errorOutput.ToString().ShouldBeEmpty();
        _standardOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void LogCommandWithUnknownOptionResultsInErrorMessage()
    {
        Program.Main(["log", "--foo"]);

        _errorOutput.ToString().ShouldBe($"Missing value for argument 'lines'{Environment.NewLine}Unrecognised args: --foo{Environment.NewLine}");
        _standardOutput.ToString().ShouldMatchSnapshot();
    }

    [Test]
    public void LogCommandWithUnparseableLinesValueResultsInException()
    {
        var ex = Assert.Throws<FormatException>(() => Program.Main(["log", "x"]));

        ex.Message.ShouldBe("The input string 'x' was not in a correct format.");
    }
}