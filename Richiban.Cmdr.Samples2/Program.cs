using System;
using System.CommandLine;
using Richiban.Cmdr.Sample;

var rootCommand = new RootCommand
{
    new CleanActions().ToCommand(),
    new BranchActions().ToCommand(),
    new RemoteActions().ToCommand()
};

rootCommand.Description = @"
__________                __    
\______   \_____    ____ |  | __
 |    |  _/\__  \  /    \|  |/ /
 |    |   \ / __ \|   |  \    < 
 |______  /(____  /___|  /__|_ \
        \/      \/     \/     \/
";

if (args is { Length: 1 } && args[0] is "--interactive" or "-i")
{
    var repl = new Repl(rootCommand, "What would you like to do?");
    repl.EnterLoop();

    return 0;
}
else
{
    return await rootCommand.InvokeAsync(args);
    return rootCommand.Invoke(args);
}