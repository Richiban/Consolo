using System;
using Cmdr;

[Cmdr("greet", Description = "A collection of commands for greeting people and pets")]
class Greeter
{
    [Cmdr("", Description = "Greets a person.")]
    public static void GreetPerson([Cmdr(Description = "The name of the person you would like to greet")] string? name = "stranger")
    {
        Console.WriteLine($"Hello, {name}!");
    }
}