using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Xml;

namespace Richiban.Cmdr.Sample
{
    internal class Repl
    {
        private const int IndentationSize = 4;

        private readonly Dictionary<char, (string helpText, Action<Repl> action)>
            _commands = new();

        private readonly int _level;

        private readonly string _promptText;
        private bool _isEnd;

        public Repl(Command command, string promptText, int level = 0)
        {
            _promptText = promptText;
            _level = level;

            var valueTuples = command.OfType<Command>()
                .ToDictionary(
                    c => c.Name[0],
                    c =>
                    {
                        return (helpText: c.Name, action: (Action<Repl>)(r => Act(c, r)));
                    });

            if (command is RootCommand)
            {
                valueTuples.Add(
                    '/',
                    (helpText: "Quit", action: _ => Environment.Exit(0)));
            }
            else
            {
                valueTuples.Add('/', (helpText: "Go back", action: r => r.End()));
            }

            _commands = valueTuples;
        }

        private void Act(Command command, Repl repl)
        {
            if (command.OfType<Command>().Any())
            {
                var subRepl = new Repl(command, command.Name, _level + 1);
                subRepl.EnterLoop();
            }
            else
            {
                var arguments = command.OfType<Argument>()
                    .Select(
                        a =>
                        {
                            Write($"{a.Name}: ");

                            return PromptString(inline: false);
                        });

                var options = command.OfType<Option>()
                    .SelectMany(
                        o =>
                        {
                            Write($"{o.Name} [y/*]: ");

                            var promptChar = PromptChar();
                            
                            System.Console.WriteLine();

                            switch (promptChar)
                            {
                                case 'y' : return new [] {o.Aliases.First()};
                                case var _: return Array.Empty<string>();
                            }
                        })
                    .ToArray();

                command.Handler?.InvokeAsync(
                        new InvocationContext(
                            new Parser(command.ToArray()).Parse(arguments.Concat(options).ToArray())))
                    .Wait();
            }
        }

        private string Indentation => new(c: ' ', _level * IndentationSize);

        public void End()
        {
            _isEnd = true;
        }

        public void Write(object toWrite) =>
            System.Console.Write($"{Indentation}{toWrite}");

        public void WriteLine(object toWrite) =>
            System.Console.WriteLine($"{Indentation}{toWrite}");

        public void EnterLoop()
        {
            while (_isEnd == false)
            {
                WriteLine("-------------------");

                if (_promptText != null)
                {
                    WriteLine(_promptText);
                }

                foreach (var (triggerKey, (helpText, action)) in _commands)
                {
                    WriteLine($"[{triggerKey}] {helpText}");
                }

                System.Console.WriteLine();

                var input = PromptChar(_commands.Keys.ToHashSet());

                System.Console.WriteLine();

                _commands[input].action(this);
            }
        }

        private string PromptString(bool inline = false)
        {
            List<char> result = new();

            ConsoleKeyInfo lastInput;

            do
            {
                lastInput = System.Console.ReadKey(intercept: true);

                if (lastInput.KeyChar is >= 'a' and <= 'z')
                {
                    result.Add(lastInput.KeyChar);
                    System.Console.Write(lastInput.KeyChar);
                }
            } while (lastInput.Key != ConsoleKey.Enter &&
                     lastInput.Key != ConsoleKey.Tab);

            if (inline == false && lastInput.Key == ConsoleKey.Enter)
            {
                System.Console.WriteLine();
            }

            return new String(result.ToArray());
        }

        private char PromptChar(ISet<char> allowedCharacters = null)
        {
            allowedCharacters ??= "abcdefghijklmnopqrstuvwxyz".ToHashSet();
            
            Write("[ ]");

            System.Console.SetCursorPosition(
                System.Console.CursorLeft - 2,
                System.Console.CursorTop);

            char input;

            do
            {
                input = System.Console.ReadKey(intercept: true).KeyChar;
            } while (!allowedCharacters.Contains(input));

            System.Console.Write(input);

            return input;
        }
    }
}