begin:

switch (args.NormaliseArgs())
{
    case ["branch", "--help"]:
        Console.WriteLine("Help for branch branch command");
        break;
    case ["branch", var branchName, "-f" or "--force"]:
        Console.WriteLine($"Creating branch {branchName} forcefully");
        break;
    case ["branch", var branchName]:
        Console.WriteLine($"Creating branch {branchName}");
        break;
    case ["branch"]:
        Console.WriteLine("Listing branches");
        break; 
    case ["branch", ..]:
        Console.Error.WriteLine("!! Unknown branch command !!");

        args = ["branch", "--help"];

        goto begin;
    case ["checkout", "--help"]:
        Console.WriteLine($"Checkout command help:");
        Console.WriteLine($"checkout <branchName> [--force]");
        break;
    case ["checkout", var branchName]:
        Console.WriteLine($"Checking out branch {branchName}");
        break;
    case ["checkout"]:
        Console.WriteLine($"!! Missing command for checkout !!");
        args = ["checkout", "--help"];
        goto begin;
    case ["commit", var message]:
        Console.WriteLine($"Committing with message: {message}");
        break;
    default:
        Console.WriteLine($"Unknown command: {String.Join(" ", args)}");
        break;
}

public static class ArgsExtensions
{
    public static string[] NormaliseArgs(this string[] args)
    {
        var copy = args
            .SelectMany(ExpandShortForm)
            .Where(s => !String.IsNullOrEmpty(s))
            .ToArray();

        Array.Sort(copy, Compare);
        return copy;
    }

    public static IEnumerable<string> ExpandShortForm(string arg)
    {
        if (arg is ['-', not '-', ..])
        {
            foreach (var c in arg.Skip(1))
            {
                yield return $"-{c}";
            }
        }
        else
        {
            yield return arg;
        }
    }

    private static int Compare(string x, string y)
    {
        if (x[0] == '-' && y[0] != '-')
            return 1;

        if (x[0] != '-' && y[0] == '-')
            return -1;

        return 0;
    }
}