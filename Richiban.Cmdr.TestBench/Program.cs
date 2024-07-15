using System.Security.Cryptography;

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

static (IReadOnlyList<string> positionalArgs, IReadOnlyList<string> options) NormaliseArgs(string[] args)
{
    var positionalArgs = new List<string>();
    var options = new List<string>();

    foreach (var arg in args)
    {
        switch (arg)
        {
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

    options.Sort((x, y) =>
        (x, y) switch
        {
            ("--help" or "-h", _) => 1,
            (_, "--help" or "-h") => -1,
            (['-', ..], [not '-', ..]) => 1,
            ([not '-', ..], ['-', ..]) => -1,
            _ => 0,
        });

    return (positionalArgs, options);
}