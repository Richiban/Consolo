using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Richiban.Cmdr.Sample
{
    public class RemoteActions
    {
        public Command ToCommand()
        {
            var cleanCommand = new Command("remote")
            {
                new AddRemoteAction().ToCommand(),
                new RemoveRemoteAction().ToCommand()
            };

            cleanCommand.Handler = CommandHandler.Create<bool>(Act);

            return cleanCommand;
        }

        private void Act(bool delete)
        {
            Console.WriteLine($"Running remote action");
            Console.WriteLine($"Delete: {delete}");
        }

        public class AddRemoteAction
        {
            public Command ToCommand()
            {
                var addRemoteCommand = new Command("add") { new Argument("remoteName") };

                addRemoteCommand.Handler = CommandHandler.Create<string>(Act);

                return addRemoteCommand;
            }

            public void Act(string remoteName)
            {
                Console.WriteLine($"Adding remote: {remoteName}");
            }
        }

        public class RemoveRemoteAction
        {
            public Command ToCommand()
            {
                var removeRemoteCommand = new Command("remove") { new Argument("remoteName") };

                removeRemoteCommand.Handler = CommandHandler.Create<string>(Act);

                return removeRemoteCommand;
            }
            
            public void Act(string remoteName)
            {
                Console.WriteLine($"Removing remote: {remoteName}");
            }
        }
    }
}