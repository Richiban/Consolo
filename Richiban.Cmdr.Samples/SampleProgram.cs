using System;
using Cmdr;
using System.IO;

namespace Richiban.Cmdr.Samples;

/// <summary>
/// A collection of actions for adding and removing remotes
/// </summary>
[Cmdr("remote")]
class CheckoutActions
{
    [Cmdr("add")]
    public static void CheckoutBranch(
        string name,
        string url,
        [Cmdr(ShortForm = "f")] bool force = false)
    {
        Console.WriteLine(new {
            name,
            url,
            force
        });
    }
}