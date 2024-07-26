using System;
using System.IO;


namespace Consolo.Samples;

/// <summary>
/// A collection of test commands
/// </summary>
[Consolo("test")]
public class TestCommands
{
    /// <summary>
    /// A test command that takes a file as a parameter
    /// </summary>
    /// <param name="file1">The first file</param>
    /// <param name="file2">The second file</param>
    /// <param name="display">The display mode</param>
    /// <param name="flag">A test flag</param>
    [Consolo("")]
    public static void TestMethod(
        FileInfo file1,
        FileInfo? file2 = null,
        [Consolo(ShortForm = "d")] DisplayValue display = DisplayValue.Long,
        [Consolo(ShortForm = "f")] bool flag = false)
    {
        Console.WriteLine($"file1: {file1.Name}");
        Console.WriteLine($"file2: {file2?.Name ?? "<null>"}");
        Console.WriteLine($"display: {display}");
        Console.WriteLine($"flag: {flag}");
    }

    public enum DisplayValue { Long, Short }
}