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
/// 
/// What does this do?
/// </summary>
[Cmdr("test")]
class TestActions
{
    /// <summary>
    /// This is a test method
    /// </summary>
    /// <param name="arg1">Argument one</param>
    /// <param name="arg2">Argument two</param>
    /// <param name="arg3"></param>
    /// <param name="flag"></param>
    [Cmdr("test2")]
    public static void TestMethod(
        string arg1,
        string arg2 = "default2",
        string arg3 = "default3",
        [Cmdr(ShortForm = "a")] bool flagA = false,
        [Cmdr(ShortForm = "b")] bool flagB = false)
    {
        Console.WriteLine(new {
            arg1,
            arg2,
            arg3,
            flagA,
            flagB
        });
    }
    
    [Cmdr("test3")]
    public static void TestMethod3(
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