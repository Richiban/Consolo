using Consolo;

namespace Consolo.TestBench;

public static class Commands
{
    public static string LastMessage { get; private set; } = string.Empty;

    [Consolo("echo")]
    public static void Echo(string message) => LastMessage = message;
}
