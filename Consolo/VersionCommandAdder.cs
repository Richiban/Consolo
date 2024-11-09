using System.Linq;
using Microsoft.CodeAnalysis;

namespace Consolo;

internal static class VersionCommandAdder
{
    private static CommandTree.SubCommand _versionCommand = new("--version", showInHelp: false)
    {
        Method = new CommandMethod(MethodName: "PrintVersion", FullyQualifiedClassName: "", Parameters: [],
            Description: "Prints the version of the application"),
        Description = "Prints the version of the application"
    };

    internal static void AddVersionCommand(CommandTree.Root rootCommand)
    {
        if (!rootCommand.SubCommands.Any(com => com.CommandName == "--version"))
        {
            rootCommand.SubCommands.Add(_versionCommand);
        }
    }
}