using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

var consoleColor = Console.ForegroundColor;
var helpTextColor = ConsoleColor.Green;
// Found 1 commands
// root command
    // checkout command

var (positionalArgs, options, isHelp) = NormaliseArgs(args);

if (positionalArgs.Length >= 0)
{
    if (positionalArgs.Length >= 1 && positionalArgs[0] == "checkout")
    {
        if (positionalArgs.Length >= 2 && positionalArgs.Length <= 2 && !isHelp)
        {
            // Found 2 parameters
            var branchName = positionalArgs[1];
            var force = options.Contains("--force") || options.Contains("-f");
            Consolo.Samples.CheckoutActions.CheckoutBranch(branchName, force);
            return;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Invalid number of arguments for command 'checkout'");
            Console.ForegroundColor = consoleColor;
        }
        Console.WriteLine(
            """
            Consolo.Samples

            Command:
                checkout <branchName> [-f | --force]
            """
        );
        Console.ForegroundColor = helpTextColor;
        Console.WriteLine(
            """

                Check out a branch
            """
        );
        Console.ForegroundColor = consoleColor;
        Console.WriteLine(
            """

            Parameters:
            """
        );
        Console.Write("    branchName    ");
        Console.ForegroundColor = helpTextColor;
        Console.WriteLine("The name of the branch to check out");
        Console.ForegroundColor = consoleColor;
        Console.Write("    -f | --force  ");
        Console.ForegroundColor = helpTextColor;
        Console.WriteLine("Allow the checkout to overwrite local changes");
        Console.ForegroundColor = consoleColor;
        return;
    }
    if (positionalArgs.Length >= 0 && positionalArgs.Length <= 0 && !isHelp)
    {
        // Found 0 parameters
        Console.WriteLine(
            """
            Consolo.Samples

            Command:

            """
        );
        return;
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine("Invalid number of arguments");
        Console.ForegroundColor = consoleColor;
    }
    Console.WriteLine(
        """
        Consolo.Samples

        Command:

        """
    );
    return;
}

static (ImmutableArray<string> positionalArgs, ImmutableArray<string> options, bool isHelp) NormaliseArgs(string[] args)
{
    var positionalArgs = new List<string>();
    var options = new List<string>();
    var isHelp = false;

    foreach (var arg in args)
    {
        switch (arg)
        {
            case "-h" or "--help":
                isHelp = true;
                break;
            case ['-', '-', ..]:
                options.Add(arg);
                break;
            case ['-', not '-', ..]:
                options.AddRange(arg.Skip(1).Select(c => $"-{c}"));
                break;
            case not (null or ""):
                positionalArgs.Add(arg);
                break;
            default:
                break;
        }
    }

    return (positionalArgs.ToImmutableArray(), options.ToImmutableArray(), isHelp);
}
