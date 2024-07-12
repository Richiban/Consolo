using System;
using Cmdr;

namespace Richiban.Cmdr.Samples;

class Program
{
    /// <summary>
    /// A function that greets a person with the appropriate formality
    /// </summary>
    /// <param name="name">The name of the person you would like to greet</param>
    /// <param name="formal">"true" means the person will be greeted very formally</param>
    [Cmdr("greet")]
    public static void GreetPersonWithTitle(string name, bool formal)
    {
        if (formal)
        {
            Console.WriteLine($"Good day to you, {name}!");
        }
        else
        {
            Console.WriteLine($"Hey, {name}!");
        }
    }
}