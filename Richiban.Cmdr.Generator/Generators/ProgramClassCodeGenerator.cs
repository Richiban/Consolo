using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Transformers;
using Richiban.Cmdr.Writers;

namespace Richiban.Cmdr.Generators
{
    internal class ProgramClassCodeGenerator : ICodeGenerator
    {
        private readonly IReadOnlyCollection<MethodModel> _methodModels;

        public ProgramClassCodeGenerator(IReadOnlyCollection<MethodModel> methodModels)
        {
            _methodModels = methodModels;
        }

        public IEnumerable<string> GetCodeLines()
        {
            var commandModel = new CommandModelTransformer().Transform(_methodModels);
            var commandCode = new CommandCodeGenerator(commandModel).GetCodeLines();

            yield return "using System;";
            yield return "using System.CommandLine;";
            yield return "using System.CommandLine.Invocation;";
            yield return "using Richiban.Cmdr;";

            yield return "";

            yield return "public static class Program";
            yield return "{";

            yield return "    public static int Main(string[] args) // WOot";

            yield return "    {";

            foreach (var line in commandCode)
            {
                yield return line;
            }

            yield return "    ";
            yield return "        if (args.Length == 1 && (args[0] == \"--interactive\" || args[0] == \"-i\"))";
            yield return "        {";
            yield return "            var repl = new Repl(rootCommand, \"Select a command\");";
            yield return "            repl.EnterLoop();";
            yield return "";
            yield return "            return 0;";
            yield return "        }";
            yield return "        else";
            yield return "        {";
            yield return "            return rootCommand.Invoke(args);";
            yield return "        }";
            yield return "    }";
            yield return "}";
        }
    }
}