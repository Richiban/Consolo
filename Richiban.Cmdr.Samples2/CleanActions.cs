using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Richiban.Cmdr.Sample
{
    public class CleanActions
    {
        /// <summary>
        /// Cleans the working directory
        /// </summary>
        /// <param name="removeDirectories">Removes empty directories</param>
        /// <param name="force"></param>
        /// <param name="ignoreIgnore"></param>
        //[CommandLine, Route]
        public void Clean(
            //[ShortForm('d', DisallowLongForm = true)] 
            bool removeDirectories = false,
            //[ShortForm('f', DisallowLongForm = true)] 
            bool force = false,
            //[ShortForm('x', DisallowLongForm = true)] 
            bool ignoreIgnore = false)
        {
            Console.WriteLine($"Cleaning working directory ({new { removeDirectories, force, ignoreIgnore }})");
        }

        public Command ToCommand()
        {
            var cleanCommand = new Command("clean")
            {
                new Option(new [] {"x", "ignoreIgnore"}),
                new Option(new [] {"d", "removeDirectories"}),
                new Option(new [] {"f", "force"})
            };
            
            cleanCommand.Handler = CommandHandler.Create<bool, bool, bool>(Clean);

            return cleanCommand;
        }
    }
}