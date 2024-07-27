using System;

namespace Consolo.Samples;

/// <summary>
/// A collection of command for git log
/// </summary>
[Consolo("log")]
class TestCommands
{
    /// <summary>
    /// A test command that takes a file as a parameter
    /// </summary>
    /// <param name="lines">The number of lines to display</param>
    /// <param name="prettyPrintMode">The display mode</param>
    [Consolo("")]
    public static void TestMethod(
        int lines,
        [Consolo("pretty")] PrettyPrintMode prettyPrintMode = PrettyPrintMode.Normal)
    {
        Console.WriteLine($"Displaying {lines} lines (pretty = {prettyPrintMode})");
    }

    public enum PrettyPrintMode 
    {
        /// <summary>
        /// Show log messages in their entirety
        /// </summary>
        Normal, 

        /// <summary>
        /// Show the first line of each log message
        /// </summary>
        OneLine 
    }
}

[Consolo("greet")]
class GreetCommands
{
    /// <summary>
    /// Greets a person
    /// </summary>
    /// <param name="name">The name of the person to greet</param>
    /// <param name="loud">Whether or not to say it loud!</param>
    [Consolo("")]
    public static void Greet(
        string name,
        bool loud = false)
    {
        Console.WriteLine($"Hello, {name}{(loud ? "!" : ".")}");
    }
}