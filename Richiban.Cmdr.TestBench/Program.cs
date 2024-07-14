begin:

switch (NormaliseArgs(args))
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

static string[] NormaliseArgs(string[] args)
{
    var copy = args
        .SelectMany(s => s switch {
            ['-', not '-', ..] => s.Skip(1).Select(c => $"-{c}"),
            _ => [s],
        })
        .Where(s => !String.IsNullOrEmpty(s))
        .ToArray();

    Array.Sort(copy, (x, y) =>
        (x, y) switch
        {
            (['-', ..], [not '-', ..]) => 1,
            ([not '-', ..], ['-', ..]) => -1,
            _ => 0,
        }
    );

    return copy;
}