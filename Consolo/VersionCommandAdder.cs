using System.Linq;

using Microsoft.CodeAnalysis;

namespace Consolo;

internal static class VersionCommandAdder
{
    internal static void AddVersionCommand(CommandTree.Root rootCommand)
    {
        switch (rootCommand.SubCommands.FirstOrDefault(com => com.CommandName == "--version"))
        {
            case null:
                rootCommand.SubCommands.Add(
                    new CommandTree.SubCommand("--version", showInHelp: false)
                    {
                        Method = new CommandMethod(
                            MethodName: "PrintVersion",
                            FullyQualifiedClassName: "",
                            Parameters: [],
                            Description: "Prints the version of the application"),
                        Description = "Prints the version of the application"
                    }
                );
                break;
        }
    }
}