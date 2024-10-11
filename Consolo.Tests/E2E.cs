using System.Text;
using System.IO;
using Consolo.Samples;

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

    [Test]
    public void LogCommandWithPrettyArgumentRunsCommand()
    {
        Program.Main(["log", "1", "--pretty", "oneline"]);

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
}