using System;
using Cmdr;
using System.IO;

namespace Richiban.Cmdr.Samples;

class RootActions
{
    [Cmdr("")]
    public static void Help()
    {
        Console.WriteLine("Help requested!");
    }
}

/// <summary>
/// These are some test actions
/// </summary>
[Cmdr("test")]
class TestActions
{
    [Cmdr("test2")]
    public static void TestMethod(
        string arg1,
        string arg2 = "default2",
        string arg3 = "default3",
        bool flag = false)
    {
        Console.WriteLine(new {
            arg1,
            arg2,
            arg3,
            flag
        });
    }
}