using System;
using Cmdr;

namespace Richiban.Cmdr.Samples;

class Program
{
    /// <summary>
    /// This is an XML comment
    /// </summary>
    /// <param name="name">The XML comment for the name</param>
    [Cmdr("greet")]
    public static void GreetPerson(string name)
    {
        Console.WriteLine($"Hello, {name}");
    }
}