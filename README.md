Cmdr
====

Cmdr is a library that makes it super easy to define command line interfaces in C#.

It offers all the features you'd expect from a command-line library such as converting command-line arguments and auto-generated help but, since it's based on a source generator, it offers a number of advantages over existing solutions (such as [`System.CommandLine`](https://www.nuget.org/packages/System.CommandLine)), such as:
* **No need to define a `Main` method**: Cmdr will generate this for you.
* **Almost no boilerplate**: All you need is to decorate your code with a few attributes and Cmdr will generate all the code you need
* **Compile-time errors**: Misconfiguration of your commands will result in errors at compile time, rather than at runtime
* **Fast**: Since the code is generated at compile time, there's no runtime overhead like reflection. Cmdr ensures that the resulting application is as fast as it's possible to be

# Installation

To get working with `Cmdr`, simply install the `Cmdr` package in your console application:

```bash
dotnet add package Cmdr
```

Or, alternatively, add the following to your `csproj` file:

```xml
    <ItemGroup>
        <PackageReference Include="Cmdr"
                          OutputItemType="Analyzer"
                          ReferenceOutputAssembly="false" />
    </ItemGroup>
```

> Note that to make use of Cmdr your console app must **not** have a `Main` method or any top-level statements;Cmdr will generate these for you.

If you want your XML comments to flow through to the generated code, you will need to add the following to your `csproj` file:

```xml
    <PropertyGroup>
        <GenerateDocumentationFile>true</GenerateDocumentationFile>
    </PropertyGroup>
```

# A Hello World example

What you write:

```cs
using System;
using Cmdr;

namespace Samples;

class Commands
{
    /// <summary>
    /// A function that can greet a person by name.
    /// 
    /// Use it to say Hello!
    /// </summary>
    /// <param name="name">The name of the person you would like to greet</param>
    [Cmdr("greet")]
    public static void GreetPerson(string name)
    {
        Console.WriteLine($"Hello, {name}");
    }
}
```

What gets generated (some sections omitted for brevity):

```cs
var (positionalArgs, options, isHelp) = NormaliseArgs(args);

if (positionalArgs.Length >= 1 && positionalArgs[0] == "greet")
{
    if (!isHelp)
    {
        if (positionalArgs.Length >= 2 && positionalArgs.Length <= 2)
        {
            var name = positionalArgs[1];
            Samples.Commands.GreetPerson(name);
            return;
        }

        if (positionalArgs.Length < 2)
        {
            // Handle missing arguments...
        }

        if (positionalArgs.Length > 2)
        {
            // Handle extraneous arguments...
        }
    }

    Console.WriteLine(
        """
        Cmdr.Samples

        Command:
            greet <name>
        """
    );
    Console.ForegroundColor = helpTextColor;
    Console.WriteLine(
        """

            A function that can greet a person by name.\n    \n    Use it to say Hello!
        """
    );
    Console.ForegroundColor = consoleColor;
    Console.WriteLine(
        """

        Parameters:
        """
    );
    Console.Write("    name  ");
    Console.ForegroundColor = helpTextColor;
    Console.WriteLine("The name of the person you would like to greet");
    Console.ForegroundColor = consoleColor;
    return;
}

Console.WriteLine("Cmdr.Samples");
Console.WriteLine("");
Console.WriteLine("Commands:");
Console.Write("    greet <name>");
Console.ForegroundColor = helpTextColor;
Console.WriteLine("  A function that can greet a person by name.\n    \n    Use it to say Hello!");
Console.ForegroundColor = consoleColor;
return;


// A few helper methods go here...
```

> As you can see, the code above that gets would be very tedious to write by hand, but Cmdr generates it all for you.

We can then call our application with the `greet` command:

```bash
> sample-app greet Alex
Hello, Alex!
```

Note that we also get the auto-generated help from System.CommandLine, with the XML comments for the method and its parameters flowing through to the generated code:

```
> sample-app greet --help
greet
  A function that can greet a person by name.

      Use it to say Hello!

Usage:
  Cmdr.Samples [options] greet <name>

Arguments:
  <name>  The name of the person you would like to greet

Options:
  -?, -h, --help  Show help and usage information
```

# More complex examples

You can define commands with multiple arguments, options, and subcommands. Here's an example of a command that uses an optional parameter:

```cs
using System;
using Cmdr;

namespace Cmdr.Samples;

class Program
{
    /// <summary>
    /// A function that can greet a person by name, with an optional title.
    /// 
    /// Use it to say Hello!
    /// </summary>
    /// <param name="name">The name of the person you would like to greet</param>
    /// <param name="title">The title of the person you would like to greet</param>
    [Cmdr("greet")]
    public static void GreetPersonWithTitle(string name, string title = "Mr")
    {
        Console.WriteLine($"Hello, {title} {name}");
    }
}
```

Let's see the help that we get for this command:

```bash
> sample-app greet --help
greet
  A function that can greet a person by name, with an optional title.

      Use it to say Hello!

Usage:
  Cmdr.Samples [options] greet <name> [<title>]

Arguments:
  <name>   The name of the person you would like to greet
  <title>  The title of the person you would like to greet [default: Mr]

Options:
  -?, -h, --help  Show help and usage information
```

We can see how it looks when we make use of "options" or "flags" in our command:

```cs
/// <summary>
/// A function that greets a person with the appropriate formality
/// </summary>
/// <param name="name">The name of the person you would like to greet</param>
/// <param name="formal">"true" means the person will be greeted very formally</param>
[Cmdr("greet", Description = "Test")]
public static void GreetPersonWithTitle(string name, [Cmdr(ShortForm = "f")] bool formal)
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
```

```
greet
  A function that greets a person with the appropriate formality

Usage:
  Cmdr.Samples [options] greet <name>

Arguments:
  <name>  The name of the person you would like to greet

Options:
  -f, --formal    "true" means the person will be greeted very formally
  -?, -h, --help  Show help and usage information
```

# Arguments and auto-conversion

Cmdr will automatically convert your arguments to the correct type, so arguments can be of any type that can be converted from a string. Specifically, any type that is not `System.String` needs to have at least one of the following:
* A `Parse` method that takes a single `System.String` argument
* A cast operator (whether implicit or explicit) that takes a single `System.String` argument
* A constructor that takes a single `System.String` argument

> Note: The conversion strategy will go through the possibilities in the order listed above. If none of these are available, an error will be given stating that the parameter type could not be converted.

For example, you can define a command that takes an integer argument:

```cs
[Cmdr("double")]
public static void DoubleMe(int number)
{
    Console.WriteLine($"The number you provided was {number}, and double that is {number * 2}");
}
```

And Cmdr will use this in the generated code:

```cs
if (positionalArgs.Length >= 2 && positionalArgs.Length <= 2)
{
    var number = System.Int32.Parse(positionalArgs[1]);
    Samples.Commands.DoubleMe(number);
    return;
}
```

Or a command that takes a `DateTime`:

```cs
[Cmdr("date")]
public static void PrintDate(DateTime date)
{
    Console.WriteLine($"The date you provided was {date.ToShortDateString()}");
}
```

Or a `FileInfo`: 

```cs
[Cmdr("file")]
public static void PrintFileInfo(FileInfo file)
{
    Console.WriteLine($"The file you provided was '{file.FullName}', and it has {file.Length} bytes");
}
```

Enums are also supported:

```cs
public enum Operation
{
    Add, Subtract, Multiply, Divide
}

/// <summary>
/// Performs a mathematical operation on two numbers
/// </summary>
[Cmdr("")]
public static void Maths(Operation op, int x, int y)
{
    switch (op)
    {
        case Operation.Add:
            Console.WriteLine($"{x} + {y} = {x + y}");
            break;
        case Operation.Subtract:
            Console.WriteLine($"{x} - {y} = {x - y}");
            break;
        case Operation.Multiply:
            Console.WriteLine($"{x} * {y} = {x * y}");
            break;
        case Operation.Divide:
            Console.WriteLine($"{x} / {y} = {x / y}");
            break;
    }
}
```

You can see that the possible enum values are reflected in the help:

```
Cmdr.Samples
    Performs a mathematical operation on two numbers

Usage:
    Cmdr.Samples <op> <x> <y> [options]

Parameters:
    op: Add|Subtract|Multiply|Divide  Operation     
    x                                 Int32
    y                                 Int32

Options:
    -h | --help  Show help and usage information    

```