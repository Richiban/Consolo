using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Richiban.Cmdr.Sample
{
    public class BranchActions
    {
        public Command ToCommand()
        {
            var cleanCommand = new Command("branch")
            {
                new Option(new [] {"D", "delete"}),
            };
            
            cleanCommand.Handler = CommandHandler.Create<bool>(Act);

            return cleanCommand;
        }

        private void Act(bool delete)
        {
            Console.WriteLine($"Running branch action");
            Console.WriteLine($"Delete: {delete}");
        }
    }
}