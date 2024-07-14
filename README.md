Cmdr
====

Cmdr is a source generator designed to work in conjunction with the [`System.CommandLine`](https://www.nuget.org/packages/System.CommandLine) library to make it super easy to define command line interfaces in C#.

# Background

The [`System.CommandLine`](https://www.nuget.org/packages/System.CommandLine) library has many things going for it; once you've defined your commands you get a lot of functionality for free, such as help text, tab completion, and argument parsing. However, defining commands can be a bit verbose, especially if you have a lot of them. Cmdr aims to make defining commands as easy as possible by generating the necessary code for you, based simply on your defining methods and decorating them with the `Cmdr` attribute. `Cmdr` will then take care of the rest!

# Installation

To get working with `Cmdr`, simply install the `Cmdr` and `System.CommandLine` packages in your console application:

```bash
dotnet add package System.CommandLine
dotnet add package Cmdr
```

Or, alternatively, add the following to your `csproj` file:

```xml
    <ItemGroup>
        <PackageReference Include="Cmdr"
                          OutputItemType="Analyzer"
                          ReferenceOutputAssembly="false" />
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="System.CommandLine"
                          Version="2.0.0-beta1.21308.1" />
    </ItemGroup>
```

> Note that to make use of Cmdr you must not have a `Main` method or top-level statements in your application. This is because Cmdr will generate these for you.

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

namespace Richiban.Cmdr.Samples;

class Program
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

What gets generated:

```cs
using System;
using System.CommandLine;
using System.CommandLine.Invocation;

var greetPersonCommand = new Command("greet", description: "A function that can greet a person by name.\n    \n    Use it to say Hello!")
{
    new Argument<System.String>(
        name: "name",
        description: "The name of the person you would like to greet")
};

greetPersonCommand.Handler = CommandHandler.Create<System.String>(Richiban.Cmdr.Samples.Program.GreetPerson);

var rootCommand = new RootCommand()
{
    greetPersonCommand
};

return rootCommand.Invoke(args);
```

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
  Richiban.Cmdr.Samples [options] greet <name>

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

namespace Richiban.Cmdr.Samples;

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
  Richiban.Cmdr.Samples [options] greet <name> [<title>]

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
public static void GreetPersonWithTitle(string name, [Cmd(Description = "Extraneous description!")] bool formal)
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
  Richiban.Cmdr.Samples [options] greet <name>

Arguments:
  <name>  The name of the person you would like to greet

Options:
  -f, --formal    "true" means the person will be greeted very formally
  -?, -h, --help  Show help and usage information
```