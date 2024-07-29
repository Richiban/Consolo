Consolo
====

Consolo is a library that makes it super easy to define command line interfaces in C#.

It offers all the features you'd expect from a command-line library (such as routing the arguments to the correct command, converting command-line arguments to the correct type, and auto-generated help) but, since it's based on a source generator, it offers a number of advantages over existing solutions like [`System.CommandLine`](https://www.nuget.org/packages/System.CommandLine):
* **No need to define a `Main` method**: Consolo will generate this for you.
* **Almost no extra code**: All you need is to decorate your actual application code with a few attributes and Consolo will generate all the code you need to turn it into a console app
* **Compile-time errors**: Misconfiguration of your commands will result in compiler errors, rather than exceptions at runtime
* **Fast**: Since most of the work is being done during the build, there's no runtime overhead like reflection. Consolo ensures that the resulting application is as fast as it's possible to be

# Installation

To get started, simply install the `Consolo` package in your console application:

```bash
dotnet add package Consolo
```

Or, alternatively, add the following to your `csproj` file:

```xml
<ItemGroup>
    <PackageReference Include="Consolo" Version="0.9.1" />
</ItemGroup>
```

> Note that to make use of Consolo your console app must **not** have a `Main` method or any top-level statements; Consolo will generate these for you.

[Optional, but highly-recommended] If you want your XML comments to flow through to the generated code, you will need to add the following to your `csproj` file:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

[Optional] If you want to see the code that Consolo generates, you can add the following to your `csproj` file:

```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

You'll then find the generated code in the `obj` folder of your project, e.g. `/obj/Debug/net8.0/generated/Consolo/Consolo.ConsoloSourceGenerator/Program.g.cs`

# A Hello World example

What you write:

```cs
using System;
using Consolo;

namespace Samples;

class Commands
{
    /// <summary>
    /// A function that greets a person by name.
    /// 
    /// Use it to say Hello!
    /// </summary>
    /// <param name="name">The name of the person you would like to greet</param>
    [Consolo]
    public static void Greet(string name)
    {
        Console.WriteLine($"Hello, {name}");
    }
}
```

A sample of what gets generated (some sections omitted for brevity):

```cs
if (args.Length >= 1 && args[0] == "greet")
{
    if (!isHelp)
    {
        if (args.Length >= 2 && args.Length <= 2)
        {
            var name = args[1];
            Samples.Commands.GreetPerson(name);
            return;
        }

        if (args.Length < 2)
        {
            // Handle missing arguments...
        }

        if (args.Length > 2)
        {
            // Handle extraneous arguments...
        }
    }

    // Auto-generated help for the 'greet' command...
}

// Auto-generated help for the application...

```

As you can see, the code above that gets would be very tedious to write by hand, but Consolo generates it all for you.

We can then call our application with the `greet` command (the command name `greet` is inferred from the method name):

```
> sample-app greet Alex
Hello, Alex!
```

Note that we also get auto-generated help similar to that of System.CommandLine, with the XML comments for the method and its parameters flowing through to the generated code:

```
> sample-app greet --help
greet
    A function that can greet a person by name.

    Use it to say Hello!

Usage:
  Consolo.Samples greet <name> [options]

Arguments:
  name  The name of the person you would like to greet

Options:
  -h | --help  Show help and usage information
```

# More complex examples

You can define commands with multiple arguments, options, and subcommands; you can also override the command name or argument name by passing a string argument to the `Consolo` attribute. Here's an example of a command that uses an optional parameter:

```cs
using System;
using Consolo;

namespace Consolo.Samples;

class Program
{
    /// <summary>
    /// A function that can greet a person by name, with an optional title.
    /// 
    /// Use it to say Hello!
    /// </summary>
    /// <param name="name">The name of the person you would like to greet</param>
    /// <param name="title">The title of the person you would like to greet</param>
    [Consolo("greet")]
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
  Consolo.Samples [options] greet <name> [<title>]

Arguments:
  <name>   The name of the person you would like to greet
  <title>  The title of the person you would like to greet [default: Mr]

Options:
  -? | -h | --help  Show help and usage information
```

We can see how it looks when we make use of "options" or "flags" in our command:

```cs
/// <summary>
/// A function that greets a person with the appropriate formality
/// </summary>
/// <param name="name">The name of the person you would like to greet</param>
/// <param name="formal">"true" means the person will be greeted very formally</param>
[Consolo("greet")]
public static void GreetPersonWithTitle(string name, [Consolo(ShortForm = "f")] bool formal)
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
  Consolo.Samples greet <name> [options]

Arguments:
  <name>  The name of the person you would like to greet

Options:
  -f | --formal  "true" means the person will be greeted very formally
  -h | --help    Show help and usage information
```

# Arguments and auto-conversion

Consolo will automatically convert your arguments to the correct type, so arguments can be of any type that can be converted from a string. Specifically, any type that is not `System.String` needs to have at least one of the following:
* A `Parse` method that takes a single `System.String` argument
* A cast operator (whether implicit or explicit) that takes a single `System.String` argument
* A constructor that takes a single `System.String` argument

> Note: The conversion strategy will go through the possibilities in the order listed above. If none of these are available, a compiler error will be given stating that the parameter type is not supported.

For example, you can define a command that takes an integer argument:

```cs
[Consolo("double")]
public static void DoubleMe(int number)
{
    Console.WriteLine($"The number you provided was {number}, and double that is {number * 2}");
}
```

And Consolo will use this in the generated code:

```cs
var number = System.Int32.Parse(args[1]);
Samples.Commands.DoubleMe(number);
```

Or a command that takes a `DateTime`:

```cs
[Consolo("date")]
public static void PrintDate(DateTime date)
{
    Console.WriteLine($"The date you provided was {date.ToShortDateString()}");
}
```

Or a `FileInfo`: 

```cs
[Consolo("file")]
public static void PrintFileInfo(FileInfo file)
{
    Console.WriteLine($"The file you provided was '{file.FullName}', and it has {file.Length} bytes");
}
```

Enums are also supported:

```cs
public enum Operation
{
    /// <summary>
    /// Adds two numbers together
    /// </summary>
    Add, 
    /// <summary>
    /// Subtracts one number from another
    /// </summary>
    Subtract,
    /// <summary>
    /// Multiplies two numbers together
    /// </summary>
    Multiply,
    /// <summary>
    /// Divides one number by another
    /// </summary>
    Divide
}

/// <summary>
/// Performs a mathematical operation on two numbers
/// </summary>
[Consolo("")]
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

You can see that the possible enum values are reflected in the help, as well as the XML comments for each value:

```
Consolo.Samples
    Performs a mathematical operation on two numbers

Usage:
    Consolo.Samples <op> <x> <y> [options]

Parameters:
    op: Add|Subtract|Multiply|Divide  Operation
                                        - Add: Adds two numbers together
                                        - Subtract: Subtracts one number from another
                                        - Multiply: Multiplies two numbers together
                                        - Divide: Divides one number by another
    x                                 Int32
    y                                 Int32

Options:
    -h | --help  Show help and usage information    

```