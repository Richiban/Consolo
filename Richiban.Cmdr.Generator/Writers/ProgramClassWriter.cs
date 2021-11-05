using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr.Writers
{
    class ProgramClassWriter
    {
        private readonly GeneratorExecutionContext _context;

        public ProgramClassWriter(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void WriteToContext(IReadOnlyCollection<MethodModel> methods)
        {
            var commands = methods.Select(m => new CommandModelWriter(m).WriteString());

            var commandsString = string.Join("\n", commands);

            var usings = new UsingsModel
            {
                "System",
                "System.CommandLine",
                "System.CommandLine.Invocation",
                "Richiban.Cmdr"
            };

            var usingsString = string.Join("\n", usings.Select(u => $"using {u};"));

            var code = @$"
{usingsString}

public static class Program
{{
    public static int Main(string[] args)
    {{
        var rootCommand = new RootCommand();

        {commandsString}

        if (args is {{ Length: 1 }} && args[0] is ""--interactive"" or ""-i"")
        {{
            var repl = new Repl(rootCommand, ""Select a command"");
            repl.EnterLoop();

            return 0;
        }}
        else
        {{
            return rootCommand.Invoke(args);
        }}
    }}
}}";

            _context.AddSource("Program.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }
}